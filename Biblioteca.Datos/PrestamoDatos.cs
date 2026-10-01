using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Biblioteca.Entidades;

namespace Biblioteca.Datos
{
    public class PrestamoDatos
    {
        public async Task<int> ContarLibrosPendientesAsync(int socioId)
        {
            const string sql = @"SELECT COUNT(*) FROM dbo.Prestamos p
                                 INNER JOIN dbo.DetallePrestamo d ON d.PrestamoId = p.PrestamoId
                                 WHERE p.SocioId = @SocioId AND d.FechaDevolucion IS NULL;";
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@SocioId", SqlDbType.Int).Value = socioId;
                await conexion.OpenAsync().ConfigureAwait(false);
                return Convert.ToInt32(await comando.ExecuteScalarAsync().ConfigureAwait(false));
            }
        }

        public async Task<int> RegistrarAsync(int socioId, IList<int> libroIds, DateTime fechaPrestamo, DateTime fechaLimite)
        {
            using (var conexion = Conexion.Crear())
            {
                await conexion.OpenAsync().ConfigureAwait(false);
                using (var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        const string insertarPrestamo = @"INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
                            SELECT @SocioId, @FechaPrestamo, @FechaLimite, N'Pendiente'
                            WHERE EXISTS (SELECT 1 FROM dbo.Socios WITH (UPDLOCK, HOLDLOCK)
                                          WHERE SocioId = @SocioId AND Activo = 1);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        int prestamoId;
                        using (var comando = new SqlCommand(insertarPrestamo, conexion, transaccion))
                        {
                            comando.Parameters.Add("@SocioId", SqlDbType.Int).Value = socioId;
                            comando.Parameters.Add("@FechaPrestamo", SqlDbType.Date).Value = fechaPrestamo.Date;
                            comando.Parameters.Add("@FechaLimite", SqlDbType.Date).Value = fechaLimite.Date;
                            var id = await comando.ExecuteScalarAsync().ConfigureAwait(false);
                            if (id == null || id == DBNull.Value)
                                throw new InvalidOperationException("El socio no existe o está inactivo.");
                            prestamoId = Convert.ToInt32(id);
                        }

                        foreach (var libroId in libroIds)
                        {
                            const string descontar = @"UPDATE dbo.Libros WITH (UPDLOCK, HOLDLOCK)
                                SET Ejemplares = Ejemplares - 1
                                WHERE LibroId = @LibroId AND Activo = 1 AND Ejemplares > 0;";
                            using (var comando = new SqlCommand(descontar, conexion, transaccion))
                            {
                                comando.Parameters.Add("@LibroId", SqlDbType.Int).Value = libroId;
                                if (await comando.ExecuteNonQueryAsync().ConfigureAwait(false) != 1)
                                    throw new InvalidOperationException("Un libro no existe, está inactivo o no tiene ejemplares disponibles.");
                            }

                            const string insertarDetalle = @"INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion)
                                                             VALUES (@PrestamoId, @LibroId, NULL);";
                            using (var comando = new SqlCommand(insertarDetalle, conexion, transaccion))
                            {
                                comando.Parameters.Add("@PrestamoId", SqlDbType.Int).Value = prestamoId;
                                comando.Parameters.Add("@LibroId", SqlDbType.Int).Value = libroId;
                                await comando.ExecuteNonQueryAsync().ConfigureAwait(false);
                            }
                        }

                        transaccion.Commit();
                        return prestamoId;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<DateTime?> RegistrarDevolucionAsync(int prestamoId, int libroId, DateTime fechaDevolucion)
        {
            using (var conexion = Conexion.Crear())
            {
                await conexion.OpenAsync().ConfigureAwait(false);
                using (var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        const string buscar = @"SELECT p.FechaLimite FROM dbo.Prestamos p WITH (UPDLOCK, HOLDLOCK)
                            INNER JOIN dbo.DetallePrestamo d WITH (UPDLOCK, HOLDLOCK) ON d.PrestamoId = p.PrestamoId
                            WHERE p.PrestamoId = @PrestamoId AND d.LibroId = @LibroId AND d.FechaDevolucion IS NULL;";
                        DateTime? fechaLimite;
                        using (var comando = new SqlCommand(buscar, conexion, transaccion))
                        {
                            comando.Parameters.Add("@PrestamoId", SqlDbType.Int).Value = prestamoId;
                            comando.Parameters.Add("@LibroId", SqlDbType.Int).Value = libroId;
                            var valor = await comando.ExecuteScalarAsync().ConfigureAwait(false);
                            fechaLimite = valor == null || valor == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(valor);
                        }
                        if (!fechaLimite.HasValue)
                        {
                            transaccion.Rollback();
                            return null;
                        }

                        const string actualizarDetalle = @"UPDATE dbo.DetallePrestamo SET FechaDevolucion = @FechaDevolucion
                            WHERE PrestamoId = @PrestamoId AND LibroId = @LibroId AND FechaDevolucion IS NULL;";
                        using (var comando = new SqlCommand(actualizarDetalle, conexion, transaccion))
                        {
                            comando.Parameters.Add("@FechaDevolucion", SqlDbType.Date).Value = fechaDevolucion.Date;
                            comando.Parameters.Add("@PrestamoId", SqlDbType.Int).Value = prestamoId;
                            comando.Parameters.Add("@LibroId", SqlDbType.Int).Value = libroId;
                            if (await comando.ExecuteNonQueryAsync().ConfigureAwait(false) != 1)
                                throw new InvalidOperationException("El ejemplar ya fue devuelto.");
                        }

                        const string reponerStock = "UPDATE dbo.Libros SET Ejemplares = Ejemplares + 1 WHERE LibroId = @LibroId;";
                        using (var comando = new SqlCommand(reponerStock, conexion, transaccion))
                        {
                            comando.Parameters.Add("@LibroId", SqlDbType.Int).Value = libroId;
                            await comando.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }

                        const string cerrarPrestamo = @"UPDATE dbo.Prestamos SET Estado = N'Devuelto'
                            WHERE PrestamoId = @PrestamoId AND NOT EXISTS
                            (SELECT 1 FROM dbo.DetallePrestamo WHERE PrestamoId = @PrestamoId AND FechaDevolucion IS NULL);";
                        using (var comando = new SqlCommand(cerrarPrestamo, conexion, transaccion))
                        {
                            comando.Parameters.Add("@PrestamoId", SqlDbType.Int).Value = prestamoId;
                            await comando.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }

                        transaccion.Commit();
                        return fechaLimite;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<List<PrestamoDetalleReporte>> ReportarPorFechasAsync(DateTime desde, DateTime hasta)
        {
            const string sql = @"SELECT p.PrestamoId, s.Nombre, l.Titulo, p.FechaPrestamo,
                                        p.FechaLimite, p.Estado, d.FechaDevolucion
                                 FROM dbo.Prestamos p
                                 INNER JOIN dbo.Socios s ON s.SocioId = p.SocioId
                                 INNER JOIN dbo.DetallePrestamo d ON d.PrestamoId = p.PrestamoId
                                 INNER JOIN dbo.Libros l ON l.LibroId = d.LibroId
                                 WHERE p.FechaPrestamo >= @Desde AND p.FechaPrestamo < @Hasta
                                 ORDER BY p.FechaPrestamo, p.PrestamoId, l.Titulo;";
            var resultado = new List<PrestamoDetalleReporte>();
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Desde", SqlDbType.Date).Value = desde.Date;
                comando.Parameters.Add("@Hasta", SqlDbType.Date).Value = hasta.Date.AddDays(1);
                await conexion.OpenAsync().ConfigureAwait(false);
                using (var lector = await comando.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await lector.ReadAsync().ConfigureAwait(false))
                        resultado.Add(new PrestamoDetalleReporte
                        {
                            PrestamoId = lector.GetInt32(0), SocioNombre = lector.GetString(1),
                            LibroTitulo = lector.GetString(2), FechaPrestamo = lector.GetDateTime(3),
                            FechaLimite = lector.GetDateTime(4), Estado = lector.GetString(5),
                            FechaDevolucion = lector.IsDBNull(6) ? (DateTime?)null : lector.GetDateTime(6)
                        });
                }
            }
            return resultado;
        }
    }
}
