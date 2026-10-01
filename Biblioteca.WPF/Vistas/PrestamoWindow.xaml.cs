using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.WPF.Vistas
{
    public partial class PrestamoWindow : Window
    {
        private readonly SocioNegocio _socios = Servicios.CrearSocioNegocio();
        private readonly LibroNegocio _libros = Servicios.CrearLibroNegocio();
        private readonly PrestamoNegocio _prestamos = Servicios.CrearPrestamoNegocio();
        private readonly ObservableCollection<Libro> _agregados = new ObservableCollection<Libro>();
        private Socio _socio;

        public PrestamoWindow()
        {
            InitializeComponent();
            lstAgregados.ItemsSource = _agregados;
            _agregados.CollectionChanged += (s, e) => txtCantidad.Text = _agregados.Count + " libro(s)";
            dpFechaLimite.DisplayDateStart = DateTime.Today;
            dpFechaLimite.SelectedDate = DateTime.Today.AddDays(7);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Operacion.EjecutarAsync(contenido, async () =>
            {
                await CargarSociosAsync();
                await CargarLibrosAsync();
            });
        }

        private async void BtnBuscarSocio_Click(object sender, RoutedEventArgs e)
        {
            await Operacion.EjecutarAsync(contenido, CargarSociosAsync);
        }

        private async void BtnBuscarLibro_Click(object sender, RoutedEventArgs e)
        {
            await Operacion.EjecutarAsync(contenido, CargarLibrosAsync);
        }

        private void DgSocios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var socio = dgSocios.SelectedItem as Socio;
            if (socio == null) return;
            _socio = socio;
            txtSocio.Text = socio.Nombre + "  (DNI " + socio.DNI + ")";
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            AgregarSeleccionado();
        }

        private void DgLibros_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AgregarSeleccionado();
        }

        private void BtnQuitar_Click(object sender, RoutedEventArgs e)
        {
            var libro = lstAgregados.SelectedItem as Libro;
            if (libro != null) _agregados.Remove(libro);
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Limpiar();
        }

        private async void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            if (_socio == null)
            {
                MessageBox.Show("Selecciona un socio.", "Préstamo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!dpFechaLimite.SelectedDate.HasValue)
            {
                MessageBox.Show("Selecciona la fecha límite.", "Préstamo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var prestamoId = 0;
            await Operacion.EjecutarAsync(contenido, async () =>
            {
                prestamoId = await _prestamos.RegistrarAsync(_socio.SocioId, _agregados.Select(l => l.LibroId),
                    dpFechaLimite.SelectedDate.Value);
                await CargarLibrosAsync();
            });
            if (prestamoId == 0) return;

            MessageBox.Show("Préstamo N° " + prestamoId + " registrado para " + _socio.Nombre + " con " + _agregados.Count +
                " libro(s).", "Préstamo", MessageBoxButton.OK, MessageBoxImage.Information);
            Limpiar();
        }

        private void AgregarSeleccionado()
        {
            var libro = dgLibros.SelectedItem as Libro;
            if (libro == null) return;
            if (_agregados.Any(l => l.LibroId == libro.LibroId))
            {
                MessageBox.Show("Ese libro ya está en el préstamo.", "Préstamo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            _agregados.Add(libro);
        }

        private async Task CargarSociosAsync()
        {
            var socios = await _socios.BuscarAsync(txtFiltroSocio.Text.Trim());
            dgSocios.ItemsSource = socios.Where(s => s.Activo).ToList();
        }

        private async Task CargarLibrosAsync()
        {
            var libros = await _libros.BuscarAsync(txtFiltroLibro.Text.Trim());
            dgLibros.ItemsSource = libros.Where(l => l.Activo && l.Ejemplares > 0).ToList();
        }

        private void Limpiar()
        {
            _socio = null;
            dgSocios.SelectedItem = null;
            txtSocio.Text = "(ninguno)";
            _agregados.Clear();
            dpFechaLimite.SelectedDate = DateTime.Today.AddDays(7);
        }
    }
}
