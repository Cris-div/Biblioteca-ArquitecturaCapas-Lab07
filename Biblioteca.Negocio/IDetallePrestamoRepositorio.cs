using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio
{
    public interface IDetallePrestamoRepositorio
    {
        Task<List<PrestamoDetalleReporte>> ListarPendientesAsync(int socioId);
        Task<List<PrestamoDetalleReporte>> ListarTodosPendientesAsync();
    }
}
