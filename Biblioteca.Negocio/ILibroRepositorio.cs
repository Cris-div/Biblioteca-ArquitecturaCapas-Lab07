using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio
{
    public interface ILibroRepositorio
    {
        Task<List<Libro>> BuscarAsync(string filtro);
        Task<Libro> ObtenerPorIdAsync(int libroId);
        Task<bool> ExisteIsbnAsync(string isbn, int? excluirLibroId = null);
        Task<bool> TienePrestamosPendientesAsync(int libroId);
        Task<int> InsertarAsync(Libro libro);
        Task ActualizarAsync(Libro libro);
        Task DarDeBajaAsync(int libroId);
    }
}
