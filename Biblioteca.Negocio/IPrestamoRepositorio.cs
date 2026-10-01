using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio
{
    public interface IPrestamoRepositorio
    {
        Task<int> ContarLibrosPendientesAsync(int socioId);
        Task<int> RegistrarAsync(int socioId, IList<int> libroIds, DateTime fechaPrestamo, DateTime fechaLimite, int maxLibrosPendientes);
        Task<DateTime?> RegistrarDevolucionAsync(int prestamoId, int libroId, DateTime fechaDevolucion);
        Task<List<PrestamoDetalleReporte>> ReportarPorFechasAsync(DateTime desde, DateTime hasta);
    }
}
