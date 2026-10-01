using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Biblioteca.Entidades;

namespace Biblioteca.Datos
{
    public class LibroDatos
    {
        public async Task<List<Libro>> BuscarAsync(string filtro)
        {
            const string sql = @"SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, a.Nombre,
                                        l.Ejemplares, l.Activo
                                 FROM dbo.Libros l
                                 INNER JOIN dbo.Autores a ON a.AutorId = l.AutorId
                                 WHERE (@Filtro = N'' OR l.Titulo LIKE @Patron OR a.Nombre LIKE @Patron)
                                 ORDER BY l.Titulo;";
            var resultado = new List<Libro>();
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Filtro", SqlDbType.NVarChar, 180).Value = filtro ?? string.Empty;
                comando.Parameters.Add("@Patron", SqlDbType.NVarChar, 182).Value = "%" + (filtro ?? string.Empty) + "%";
                await conexion.OpenAsync().ConfigureAwait(false);
                using (var lector = await comando.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await lector.ReadAsync().ConfigureAwait(false)) resultado.Add(Mapear(lector));
                }
            }
            return resultado;
        }

        public async Task<Libro> ObtenerPorIdAsync(int libroId)
        {
            const string sql = @"SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, a.Nombre,
                                        l.Ejemplares, l.Activo
                                 FROM dbo.Libros l INNER JOIN dbo.Autores a ON a.AutorId = l.AutorId
                                 WHERE l.LibroId = @LibroId;";
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@LibroId", SqlDbType.Int).Value = libroId;
                await conexion.OpenAsync().ConfigureAwait(false);
                using (var lector = await comando.ExecuteReaderAsync(CommandBehavior.SingleRow).ConfigureAwait(false))
                    return await lector.ReadAsync().ConfigureAwait(false) ? Mapear(lector) : null;
            }
        }

        public async Task<bool> ExisteIsbnAsync(string isbn, int? excluirLibroId = null)
        {
            const string sql = @"SELECT CASE WHEN EXISTS
                                 (SELECT 1 FROM dbo.Libros WHERE ISBN = @ISBN
                                  AND (@ExcluirId IS NULL OR LibroId <> @ExcluirId)) THEN 1 ELSE 0 END;";
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@ISBN", SqlDbType.VarChar, 20).Value = isbn;
                comando.Parameters.Add("@ExcluirId", SqlDbType.Int).Value = (object)excluirLibroId ?? DBNull.Value;
                await conexion.OpenAsync().ConfigureAwait(false);
                return Convert.ToInt32(await comando.ExecuteScalarAsync().ConfigureAwait(false)) == 1;
            }
        }

        public async Task<bool> TienePrestamosPendientesAsync(int libroId)
        {
            const string sql = @"SELECT CASE WHEN EXISTS
                                 (SELECT 1 FROM dbo.DetallePrestamo WHERE LibroId = @LibroId AND FechaDevolucion IS NULL)
                                 THEN 1 ELSE 0 END;";
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@LibroId", SqlDbType.Int).Value = libroId;
                await conexion.OpenAsync().ConfigureAwait(false);
                return Convert.ToInt32(await comando.ExecuteScalarAsync().ConfigureAwait(false)) == 1;
            }
        }

        public async Task<int> InsertarAsync(Libro libro)
        {
            const string sql = @"INSERT INTO dbo.Libros (Titulo, ISBN, AutorId, Ejemplares, Activo)
                                 VALUES (@Titulo, @ISBN, @AutorId, @Ejemplares, 1);
                                 SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                AgregarParametros(comando, libro);
                await conexion.OpenAsync().ConfigureAwait(false);
                return Convert.ToInt32(await comando.ExecuteScalarAsync().ConfigureAwait(false));
            }
        }

        public async Task ActualizarAsync(Libro libro)
        {
            const string sql = @"UPDATE dbo.Libros SET Titulo = @Titulo, ISBN = @ISBN,
                                 AutorId = @AutorId, Ejemplares = @Ejemplares WHERE LibroId = @LibroId;";
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                AgregarParametros(comando, libro);
                comando.Parameters.Add("@LibroId", SqlDbType.Int).Value = libro.LibroId;
                await conexion.OpenAsync().ConfigureAwait(false);
                await comando.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task DarDeBajaAsync(int libroId)
        {
            const string sql = @"UPDATE dbo.Libros WITH (UPDLOCK, HOLDLOCK) SET Activo = 0
                                 WHERE LibroId = @LibroId AND Activo = 1
                                   AND NOT EXISTS (SELECT 1 FROM dbo.DetallePrestamo
                                                   WHERE LibroId = @LibroId AND FechaDevolucion IS NULL);";
            using (var conexion = Conexion.Crear())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@LibroId", SqlDbType.Int).Value = libroId;
                await conexion.OpenAsync().ConfigureAwait(false);
                if (await comando.ExecuteNonQueryAsync().ConfigureAwait(false) != 1)
                    throw new InvalidOperationException("No se puede dar de baja un libro inexistente o con préstamos pendientes.");
            }
        }

        private static void AgregarParametros(SqlCommand comando, Libro libro)
        {
            comando.Parameters.Add("@Titulo", SqlDbType.NVarChar, 180).Value = libro.Titulo;
            comando.Parameters.Add("@ISBN", SqlDbType.VarChar, 20).Value = libro.ISBN;
            comando.Parameters.Add("@AutorId", SqlDbType.Int).Value = libro.AutorId;
            comando.Parameters.Add("@Ejemplares", SqlDbType.Int).Value = libro.Ejemplares;
        }

        private static Libro Mapear(SqlDataReader lector)
        {
            return new Libro
            {
                LibroId = lector.GetInt32(0), Titulo = lector.GetString(1), ISBN = lector.GetString(2),
                AutorId = lector.GetInt32(3), NombreAutor = lector.GetString(4),
                Ejemplares = lector.GetInt32(5), Activo = lector.GetBoolean(6)
            };
        }
    }
}
