using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Biblioteca.Negocio;

namespace Biblioteca.WPF.Vistas
{
    public partial class ReporteWindow : Window
    {
        private readonly PrestamoNegocio _negocio = Servicios.CrearPrestamoNegocio();

        public ReporteWindow()
        {
            InitializeComponent();
            dpDesde.SelectedDate = DateTime.Today.AddDays(-30);
            dpHasta.SelectedDate = DateTime.Today;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Operacion.EjecutarAsync(contenido, GenerarAsync);
        }

        private async void BtnGenerar_Click(object sender, RoutedEventArgs e)
        {
            await Operacion.EjecutarAsync(contenido, GenerarAsync);
        }

        private async Task GenerarAsync()
        {
            if (!dpDesde.SelectedDate.HasValue || !dpHasta.SelectedDate.HasValue)
                throw new ReglaNegocioException("Selecciona ambas fechas del intervalo.");

            var filas = await _negocio.ReportarPorFechasAsync(dpDesde.SelectedDate.Value, dpHasta.SelectedDate.Value);
            dgReporte.ItemsSource = filas;
            var prestamos = filas.Select(f => f.PrestamoId).Distinct().Count();
            var pendientes = filas.Count(f => !f.FechaDevolucion.HasValue);
            txtResumen.Text = prestamos + " préstamo(s) · " + filas.Count + " libro(s) · " + pendientes + " pendiente(s) de devolución.";
        }
    }
}
