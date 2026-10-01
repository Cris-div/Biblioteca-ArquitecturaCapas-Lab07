using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Biblioteca.Entidades;

namespace Biblioteca.Datos
{
    public class DetallePrestamoDatos
    {
        public async Task<List<PrestamoDetalleReporte>> ListarPendientesAsync(int socioId)
        {
            const string sql = @"SELECT p.PrestamoId, d.LibroId, s.Nombre, l.Titulo, p.FechaPrestamo,
                                        p.FechaLimite, p.Estado, d.FechaDevolucion
                                 FROM dbo.Prestamos p
                                 INNER JOIN dbo.Socios s ON s.SocioId = p.SocioId
                                 INNER JOIN dbo.DetallePrestamo d ON d.PrestamoId = p.PrestamoId
                                 INNER JOIN dbo.Libros l ON l.LibroId = d.LibroId
                                 WHERE p.SocioId = @SocioId AND d.FechaDevolucion IS NULL
                                 ORDER BY p.FechaLimite, l.Titulo;";
            var resultado = new List<PrestamoDetalleReporte>();
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@SocioId", SqlDbType.Int).Value = socioId;
                await conexion.OpenAsync().ConfigureAwait(false);
                using (var lector = await comando.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await lector.ReadAsync().ConfigureAwait(false))
                        resultado.Add(new PrestamoDetalleReporte
                        {
                            PrestamoId = lector.GetInt32(0), LibroId = lector.GetInt32(1),
                            SocioNombre = lector.GetString(2), LibroTitulo = lector.GetString(3),
                            FechaPrestamo = lector.GetDateTime(4), FechaLimite = lector.GetDateTime(5),
                            Estado = lector.GetString(6),
                            FechaDevolucion = lector.IsDBNull(7) ? (System.DateTime?)null : lector.GetDateTime(7)
                        });
                }
            }
            return resultado;
        }

        public async Task<List<PrestamoDetalleReporte>> ListarTodosPendientesAsync()
        {
            const string sql = @"SELECT p.PrestamoId, d.LibroId, s.Nombre, l.Titulo, p.FechaPrestamo,
                                        p.FechaLimite, p.Estado, d.FechaDevolucion
                                 FROM dbo.Prestamos p
                                 INNER JOIN dbo.Socios s ON s.SocioId = p.SocioId
                                 INNER JOIN dbo.DetallePrestamo d ON d.PrestamoId = p.PrestamoId
                                 INNER JOIN dbo.Libros l ON l.LibroId = d.LibroId
                                 WHERE d.FechaDevolucion IS NULL
                                 ORDER BY p.FechaLimite, s.Nombre, l.Titulo;";
            var resultado = new List<PrestamoDetalleReporte>();
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                await conexion.OpenAsync().ConfigureAwait(false);
                using (var lector = await comando.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await lector.ReadAsync().ConfigureAwait(false))
                        resultado.Add(new PrestamoDetalleReporte
                        {
                            PrestamoId = lector.GetInt32(0), LibroId = lector.GetInt32(1),
                            SocioNombre = lector.GetString(2), LibroTitulo = lector.GetString(3),
                            FechaPrestamo = lector.GetDateTime(4), FechaLimite = lector.GetDateTime(5),
                            Estado = lector.GetString(6),
                            FechaDevolucion = lector.IsDBNull(7) ? (System.DateTime?)null : lector.GetDateTime(7)
                        });
                }
            }
            return resultado;
        }
    }
}
