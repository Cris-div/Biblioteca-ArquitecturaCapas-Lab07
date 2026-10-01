using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio
{
    public class PrestamoNegocio
    {
        private const decimal MultaPorDia = 1.50m;
        private readonly SocioDatos _socios;
        private readonly LibroDatos _libros;
        private readonly PrestamoDatos _prestamos;
        private readonly DetallePrestamoDatos _detalles;

        public PrestamoNegocio() : this(new SocioDatos(), new LibroDatos(), new PrestamoDatos(), new DetallePrestamoDatos()) { }

        public PrestamoNegocio(SocioDatos socios, LibroDatos libros, PrestamoDatos prestamos, DetallePrestamoDatos detalles)
        {
            _socios = socios ?? throw new ArgumentNullException(nameof(socios));
            _libros = libros ?? throw new ArgumentNullException(nameof(libros));
            _prestamos = prestamos ?? throw new ArgumentNullException(nameof(prestamos));
            _detalles = detalles ?? throw new ArgumentNullException(nameof(detalles));
        }

        public async Task<int> RegistrarAsync(int socioId, IEnumerable<int> libroIds, DateTime fechaLimite)
        {
            var ids = libroIds == null ? new List<int>() : libroIds.ToList();
            if (ids.Count == 0) throw new ReglaNegocioException("Agrega al menos un libro al préstamo.");
            if (ids.Any(id => id <= 0)) throw new ReglaNegocioException("La lista contiene un libro no válido.");
            if (ids.Distinct().Count() != ids.Count) throw new ReglaNegocioException("No agregues el mismo libro más de una vez al préstamo.");

            var socio = await _socios.ObtenerPorIdAsync(socioId).ConfigureAwait(false);
            if (socio == null || !socio.Activo) throw new ReglaNegocioException("El socio no existe o está inactivo.");

            var pendientes = await _prestamos.ContarLibrosPendientesAsync(socioId).ConfigureAwait(false);
            if (pendientes + ids.Count > 3)
                throw new ReglaNegocioException("Un socio no puede tener más de 3 libros pendientes.");

            foreach (var id in ids)
            {
                var libro = await _libros.ObtenerPorIdAsync(id).ConfigureAwait(false);
                if (libro == null || !libro.Activo) throw new ReglaNegocioException("Uno de los libros no existe o está inactivo.");
                if (libro.Ejemplares <= 0) throw new ReglaNegocioException("No hay ejemplares disponibles de '" + libro.Titulo + "'.");
            }

            var hoy = DateTime.Today;
            if (fechaLimite.Date < hoy) throw new ReglaNegocioException("La fecha límite no puede ser anterior a hoy.");
            return await _prestamos.RegistrarAsync(socioId, ids, hoy, fechaLimite.Date).ConfigureAwait(false);
        }

        public Task<List<PrestamoDetalleReporte>> ListarPendientesAsync(int socioId)
        {
            return _detalles.ListarPendientesAsync(socioId);
        }

        public async Task<decimal> RegistrarDevolucionAsync(int prestamoId, int libroId, DateTime fechaDevolucion)
        {
            var fechaLimite = await _prestamos.RegistrarDevolucionAsync(prestamoId, libroId, fechaDevolucion.Date).ConfigureAwait(false);
            if (!fechaLimite.HasValue)
                throw new ReglaNegocioException("El préstamo o libro no existe, o el ejemplar ya fue devuelto.");
            var diasRetraso = Math.Max(0, (fechaDevolucion.Date - fechaLimite.Value.Date).Days);
            return diasRetraso * MultaPorDia;
        }

        public async Task<List<PrestamoDetalleReporte>> ReportarPorFechasAsync(DateTime desde, DateTime hasta)
        {
            if (desde.Date > hasta.Date) throw new ReglaNegocioException("El inicio del intervalo no puede ser posterior al final.");
            return await _prestamos.ReportarPorFechasAsync(desde.Date, hasta.Date).ConfigureAwait(false);
        }
    }
}
