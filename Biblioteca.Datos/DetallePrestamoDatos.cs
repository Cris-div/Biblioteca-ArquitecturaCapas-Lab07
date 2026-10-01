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
            const string sql = @"SELECT p.PrestamoId, s.Nombre, l.Titulo, p.FechaPrestamo,
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
                            PrestamoId = lector.GetInt32(0), SocioNombre = lector.GetString(1),
                            LibroTitulo = lector.GetString(2), FechaPrestamo = lector.GetDateTime(3),
                            FechaLimite = lector.GetDateTime(4), Estado = lector.GetString(5),
                            FechaDevolucion = lector.IsDBNull(6) ? (System.DateTime?)null : lector.GetDateTime(6)
                        });
                }
            }
            return resultado;
        }
    }
}
