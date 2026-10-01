using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio
{
    public interface ISocioRepositorio
    {
        Task<List<Socio>> BuscarAsync(string filtro);
        Task<Socio> ObtenerPorIdAsync(int socioId);
        Task<bool> ExisteDniAsync(string dni, int? excluirSocioId = null);
        Task<bool> TienePrestamosPendientesAsync(int socioId);
        Task<int> InsertarAsync(Socio socio);
        Task ActualizarAsync(Socio socio);
        Task DarDeBajaAsync(int socioId);
    }
}
