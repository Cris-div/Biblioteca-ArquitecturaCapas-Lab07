using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.WPF.Vistas
{
    public partial class DevolucionWindow : Window
    {
        private readonly PrestamoNegocio _negocio = Servicios.CrearPrestamoNegocio();

        public DevolucionWindow()
        {
            InitializeComponent();
            dpFechaDevolucion.SelectedDate = DateTime.Today;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Operacion.EjecutarAsync(contenido, CargarPendientesAsync);
        }

        private async void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            await Operacion.EjecutarAsync(contenido, CargarPendientesAsync);
        }

        private void DgPendientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = dgPendientes.SelectedItem as PrestamoDetalleReporte;
            txtSeleccion.Text = item == null
                ? "(ninguno)"
                : item.LibroTitulo + "\n" + item.SocioNombre + " · vence " + item.FechaLimite.ToString("dd/MM/yyyy");
        }

        private async void BtnDevolver_Click(object sender, RoutedEventArgs e)
        {
            var item = dgPendientes.SelectedItem as PrestamoDetalleReporte;
            if (item == null)
            {
                MessageBox.Show("Selecciona un libro pendiente.", "Devolución", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!dpFechaDevolucion.SelectedDate.HasValue)
            {
                MessageBox.Show("Selecciona la fecha de devolución.", "Devolución", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var fecha = dpFechaDevolucion.SelectedDate.Value;
            decimal? multa = null;
            await Operacion.EjecutarAsync(contenido, async () =>
            {
                multa = await _negocio.RegistrarDevolucionAsync(item.PrestamoId, item.LibroId, fecha);
                await CargarPendientesAsync();
            });
            if (!multa.HasValue) return;

            var diasRetraso = Math.Max(0, (fecha.Date - item.FechaLimite.Date).Days);
            txtMulta.Text = "S/ " + multa.Value.ToString("N2");
            txtDetalleMulta.Text = "'" + item.LibroTitulo + "' devuelto por " + item.SocioNombre + " el " +
                fecha.ToString("dd/MM/yyyy") + ". " +
                (diasRetraso == 0 ? "Entregado a tiempo, sin multa." : diasRetraso + " día(s) de retraso × S/ 1.50.");
            panelMulta.Visibility = Visibility.Visible;
        }

        private async Task CargarPendientesAsync()
        {
            var pendientes = await _negocio.ListarPendientesAsync();
            dgPendientes.ItemsSource = pendientes;
            txtTotal.Text = pendientes.Count + " libro(s) pendientes.";
        }
    }
}
