using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio
{
    public class LibroNegocio
    {
        private readonly LibroDatos _libros;
        private readonly AutorDatos _autores;

        public LibroNegocio() : this(new LibroDatos(), new AutorDatos()) { }
        public LibroNegocio(LibroDatos libros) : this(libros, new AutorDatos()) { }
        public LibroNegocio(LibroDatos libros, AutorDatos autores)
        {
            _libros = libros ?? throw new ArgumentNullException(nameof(libros));
            _autores = autores ?? throw new ArgumentNullException(nameof(autores));
        }

        public Task<List<Libro>> BuscarAsync(string filtro) => _libros.BuscarAsync(filtro);
        public Task<List<Autor>> ListarAutoresAsync() => _autores.ListarActivosAsync();

        public async Task<int> InsertarAsync(Libro libro)
        {
            Validar(libro);
            if (await _libros.ExisteIsbnAsync(libro.ISBN).ConfigureAwait(false))
                throw new ReglaNegocioException("Ya existe un libro registrado con ese ISBN.");
            return await _libros.InsertarAsync(libro).ConfigureAwait(false);
        }

        public async Task ActualizarAsync(Libro libro)
        {
            Validar(libro);
            var actual = await _libros.ObtenerPorIdAsync(libro.LibroId).ConfigureAwait(false);
            if (actual == null || !actual.Activo)
                throw new ReglaNegocioException("No se puede actualizar un libro inexistente o dado de baja.");
            if (await _libros.ExisteIsbnAsync(libro.ISBN, libro.LibroId).ConfigureAwait(false))
                throw new ReglaNegocioException("Otro libro ya está registrado con ese ISBN.");
            await _libros.ActualizarAsync(libro).ConfigureAwait(false);
        }

        public async Task DarDeBajaAsync(int libroId)
        {
            var actual = await _libros.ObtenerPorIdAsync(libroId).ConfigureAwait(false);
            if (actual == null || !actual.Activo)
                throw new ReglaNegocioException("El libro no existe o ya está dado de baja.");
            if (await _libros.TienePrestamosPendientesAsync(libroId).ConfigureAwait(false))
                throw new ReglaNegocioException("No se puede dar de baja un libro con préstamos pendientes.");
            try
            {
                await _libros.DarDeBajaAsync(libroId).ConfigureAwait(false);
            }
            catch (InvalidOperationException ex)
            {
                throw new ReglaNegocioException(ex.Message);
            }
        }

        private static void Validar(Libro libro)
        {
            if (libro == null) throw new ReglaNegocioException("Debes indicar los datos del libro.");
            if (string.IsNullOrWhiteSpace(libro.Titulo)) throw new ReglaNegocioException("El título del libro es obligatorio.");
            if (string.IsNullOrWhiteSpace(libro.ISBN)) throw new ReglaNegocioException("El ISBN del libro es obligatorio.");
            if (libro.ISBN.Trim().Length > 20) throw new ReglaNegocioException("El ISBN no puede superar los 20 caracteres.");
            if (libro.AutorId <= 0) throw new ReglaNegocioException("Debes seleccionar un autor válido.");
            if (libro.Ejemplares < 0) throw new ReglaNegocioException("La cantidad de ejemplares no puede ser negativa.");
        }
    }
}
