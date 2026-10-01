using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Biblioteca.Entidades;

namespace Biblioteca.Datos
{
    public class AutorDatos
    {
        public async Task<List<Autor>> ListarActivosAsync()
        {
            const string sql = @"SELECT AutorId, Nombre, Nacionalidad, Activo
                                 FROM dbo.Autores WHERE Activo = 1 ORDER BY Nombre;";
            var resultado = new List<Autor>();
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                await conexion.OpenAsync().ConfigureAwait(false);
                using (var lector = await comando.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                {
                    while (await lector.ReadAsync().ConfigureAwait(false))
                    {
                        resultado.Add(new Autor
                        {
                            AutorId = lector.GetInt32(0),
                            Nombre = lector.GetString(1),
                            Nacionalidad = lector.IsDBNull(2) ? null : lector.GetString(2),
                            Activo = lector.GetBoolean(3)
                        });
                    }
                }
            }
            return resultado;
        }
    }
}
