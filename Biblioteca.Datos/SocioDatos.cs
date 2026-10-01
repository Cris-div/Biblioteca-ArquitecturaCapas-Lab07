using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.Datos
{
    public class SocioDatos : ISocioRepositorio
    {
        public async Task<List<Socio>> BuscarAsync(string filtro)
        {
            const string sql = @"SELECT SocioId, DNI, Nombre, Email, Activo FROM dbo.Socios
                                 WHERE (@Filtro = N'' OR Nombre LIKE @Patron OR DNI LIKE @Patron)
                                 ORDER BY Nombre;";
            var resultado = new List<Socio>();
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Filtro", SqlDbType.NVarChar, 160).Value = filtro ?? string.Empty;
                comando.Parameters.Add("@Patron", SqlDbType.NVarChar, 162).Value = "%" + (filtro ?? string.Empty) + "%";
                await conexion.OpenAsync().ConfigureAwait(false);
                using (var lector = await comando.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await lector.ReadAsync().ConfigureAwait(false))
                        resultado.Add(new Socio { SocioId = lector.GetInt32(0), DNI = lector.GetString(1),
                            Nombre = lector.GetString(2), Email = lector.IsDBNull(3) ? null : lector.GetString(3), Activo = lector.GetBoolean(4) });
                }
            }
            return resultado;
        }

        public async Task<Socio> ObtenerPorIdAsync(int socioId)
        {
            const string sql = "SELECT SocioId, DNI, Nombre, Email, Activo FROM dbo.Socios WHERE SocioId = @SocioId;";
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@SocioId", SqlDbType.Int).Value = socioId;
                await conexion.OpenAsync().ConfigureAwait(false);
                using (var lector = await comando.ExecuteReaderAsync(CommandBehavior.SingleRow).ConfigureAwait(false))
                {
                    if (!await lector.ReadAsync().ConfigureAwait(false)) return null;
                    return new Socio { SocioId = lector.GetInt32(0), DNI = lector.GetString(1), Nombre = lector.GetString(2),
                        Email = lector.IsDBNull(3) ? null : lector.GetString(3), Activo = lector.GetBoolean(4) };
                }
            }
        }

        public async Task<bool> ExisteDniAsync(string dni, int? excluirSocioId = null)
        {
            const string sql = @"SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Socios WHERE DNI = @DNI
                                 AND (@ExcluirId IS NULL OR SocioId <> @ExcluirId)) THEN 1 ELSE 0 END;";
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@DNI", SqlDbType.VarChar, 15).Value = dni;
                comando.Parameters.Add("@ExcluirId", SqlDbType.Int).Value = (object)excluirSocioId ?? DBNull.Value;
                await conexion.OpenAsync().ConfigureAwait(false);
                return Convert.ToInt32(await comando.ExecuteScalarAsync().ConfigureAwait(false)) == 1;
            }
        }

        public async Task<bool> TienePrestamosPendientesAsync(int socioId)
        {
            const string sql = @"SELECT CASE WHEN EXISTS
                                 (SELECT 1 FROM dbo.Prestamos p INNER JOIN dbo.DetallePrestamo d ON d.PrestamoId = p.PrestamoId
                                  WHERE p.SocioId = @SocioId AND d.FechaDevolucion IS NULL) THEN 1 ELSE 0 END;";
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@SocioId", SqlDbType.Int).Value = socioId;
                await conexion.OpenAsync().ConfigureAwait(false);
                return Convert.ToInt32(await comando.ExecuteScalarAsync().ConfigureAwait(false)) == 1;
            }
        }

        public async Task<int> InsertarAsync(Socio socio)
        {
            const string sql = @"INSERT INTO dbo.Socios (DNI, Nombre, Email, Activo) VALUES (@DNI, @Nombre, @Email, 1);
                                 SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                AgregarParametros(comando, socio);
                await conexion.OpenAsync().ConfigureAwait(false);
                return Convert.ToInt32(await comando.ExecuteScalarAsync().ConfigureAwait(false));
            }
        }

        public async Task ActualizarAsync(Socio socio)
        {
            const string sql = "UPDATE dbo.Socios SET DNI = @DNI, Nombre = @Nombre, Email = @Email WHERE SocioId = @SocioId;";
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                AgregarParametros(comando, socio);
                comando.Parameters.Add("@SocioId", SqlDbType.Int).Value = socio.SocioId;
                await conexion.OpenAsync().ConfigureAwait(false);
                await comando.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task DarDeBajaAsync(int socioId)
        {
            const string sql = @"UPDATE dbo.Socios WITH (UPDLOCK, HOLDLOCK) SET Activo = 0
                                 WHERE SocioId = @SocioId AND Activo = 1
                                   AND NOT EXISTS
                                   (SELECT 1 FROM dbo.Prestamos p INNER JOIN dbo.DetallePrestamo d ON d.PrestamoId = p.PrestamoId
                                    WHERE p.SocioId = @SocioId AND d.FechaDevolucion IS NULL);";
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@SocioId", SqlDbType.Int).Value = socioId;
                await conexion.OpenAsync().ConfigureAwait(false);
                if (await comando.ExecuteNonQueryAsync().ConfigureAwait(false) != 1)
                    throw new InvalidOperationException("No se puede dar de baja un socio inexistente o con préstamos pendientes.");
            }
        }

        private static void AgregarParametros(SqlCommand comando, Socio socio)
        {
            comando.Parameters.Add("@DNI", SqlDbType.VarChar, 15).Value = socio.DNI;
            comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 140).Value = socio.Nombre;
            comando.Parameters.Add("@Email", SqlDbType.NVarChar, 160).Value = (object)socio.Email ?? DBNull.Value;
        }
    }
}
