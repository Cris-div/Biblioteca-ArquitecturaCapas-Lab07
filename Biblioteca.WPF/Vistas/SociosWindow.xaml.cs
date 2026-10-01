using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.WPF.Vistas
{
    public partial class SociosWindow : Window
    {
        private readonly SocioNegocio _negocio = Servicios.CrearSocioNegocio();
        private Socio _seleccionado;

        public SociosWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Operacion.EjecutarAsync(contenido, CargarSociosAsync);
            Limpiar();
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            await Operacion.EjecutarAsync(contenido, CargarSociosAsync);
            Limpiar();
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var esNuevo = _seleccionado == null;
            var guardado = false;
            await Operacion.EjecutarAsync(contenido, async () =>
            {
                var socio = LeerFormulario();
                if (esNuevo)
                    await _negocio.InsertarAsync(socio);
                else
                    await _negocio.ActualizarAsync(socio);
                guardado = true;
                await CargarSociosAsync();
            });
            if (!guardado) return;
            MessageBox.Show(esNuevo ? "Socio registrado correctamente." : "Socio actualizado correctamente.",
                "Socios", MessageBoxButton.OK, MessageBoxImage.Information);
            Limpiar();
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_seleccionado == null)
            {
                MessageBox.Show("Selecciona un socio de la lista.", "Socios", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var respuesta = MessageBox.Show("¿Dar de baja al socio '" + _seleccionado.Nombre + "'?", "Confirmar",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (respuesta != MessageBoxResult.Yes) return;

            var socioId = _seleccionado.SocioId;
            await Operacion.EjecutarAsync(contenido, async () =>
            {
                await _negocio.DarDeBajaAsync(socioId);
                await CargarSociosAsync();
                Limpiar();
            });
        }

        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            Limpiar();
        }

        private void DgSocios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var socio = dgSocios.SelectedItem as Socio;
            if (socio == null) return;
            _seleccionado = socio;
            txtModo.Text = "Editar socio #" + socio.SocioId;
            txtDni.Text = socio.DNI;
            txtNombre.Text = socio.Nombre;
            txtEmail.Text = socio.Email;
            btnEliminar.IsEnabled = socio.Activo;
        }

        private async Task CargarSociosAsync()
        {
            var socios = await _negocio.BuscarAsync(txtFiltro.Text.Trim());
            dgSocios.ItemsSource = socios;
            txtTotal.Text = socios.Count + " socio(s) encontrados.";
        }

        private Socio LeerFormulario()
        {
            var email = txtEmail.Text.Trim();
            return new Socio
            {
                SocioId = _seleccionado == null ? 0 : _seleccionado.SocioId,
                DNI = txtDni.Text.Trim(),
                Nombre = txtNombre.Text.Trim(),
                Email = email.Length == 0 ? null : email
            };
        }

        private void Limpiar()
        {
            _seleccionado = null;
            dgSocios.SelectedItem = null;
            txtModo.Text = "Nuevo socio";
            txtDni.Clear();
            txtNombre.Clear();
            txtEmail.Clear();
            btnEliminar.IsEnabled = false;
        }
    }
}
