using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.WPF.Vistas
{
    public partial class LibrosWindow : Window
    {
        private readonly LibroNegocio _negocio = Servicios.CrearLibroNegocio();
        private Libro _seleccionado;

        public LibrosWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Operacion.EjecutarAsync(contenido, async () =>
            {
                cboAutor.ItemsSource = await _negocio.ListarAutoresAsync();
                await CargarLibrosAsync();
            });
            Limpiar();
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            await Operacion.EjecutarAsync(contenido, CargarLibrosAsync);
            Limpiar();
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var esNuevo = _seleccionado == null;
            var guardado = false;
            await Operacion.EjecutarAsync(contenido, async () =>
            {
                var libro = LeerFormulario();
                if (esNuevo)
                    await _negocio.InsertarAsync(libro);
                else
                    await _negocio.ActualizarAsync(libro);
                guardado = true;
                await CargarLibrosAsync();
            });
            if (!guardado) return;
            MessageBox.Show(esNuevo ? "Libro registrado correctamente." : "Libro actualizado correctamente.",
                "Libros", MessageBoxButton.OK, MessageBoxImage.Information);
            Limpiar();
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_seleccionado == null)
            {
                MessageBox.Show("Selecciona un libro de la lista.", "Libros", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var respuesta = MessageBox.Show("¿Dar de baja el libro '" + _seleccionado.Titulo + "'?", "Confirmar",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (respuesta != MessageBoxResult.Yes) return;

            var libroId = _seleccionado.LibroId;
            await Operacion.EjecutarAsync(contenido, async () =>
            {
                await _negocio.DarDeBajaAsync(libroId);
                await CargarLibrosAsync();
                Limpiar();
            });
        }

        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            Limpiar();
        }

        private void DgLibros_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var libro = dgLibros.SelectedItem as Libro;
            if (libro == null) return;
            _seleccionado = libro;
            txtModo.Text = "Editar libro #" + libro.LibroId;
            txtTitulo.Text = libro.Titulo;
            txtIsbn.Text = libro.ISBN;
            cboAutor.SelectedValue = libro.AutorId;
            txtEjemplares.Text = libro.Ejemplares.ToString();
            btnEliminar.IsEnabled = libro.Activo;
            btnGuardar.IsEnabled = libro.Activo;
        }

        private async Task CargarLibrosAsync()
        {
            var libros = await _negocio.BuscarAsync(txtFiltro.Text.Trim());
            dgLibros.ItemsSource = libros;
            txtTotal.Text = libros.Count + " libro(s) encontrados.";
        }

        private Libro LeerFormulario()
        {
            int ejemplares;
            if (!int.TryParse(txtEjemplares.Text.Trim(), out ejemplares))
                throw new ReglaNegocioException("Ingresa una cantidad de ejemplares válida.");
            return new Libro
            {
                LibroId = _seleccionado == null ? 0 : _seleccionado.LibroId,
                Titulo = txtTitulo.Text.Trim(),
                ISBN = txtIsbn.Text.Trim(),
                AutorId = cboAutor.SelectedValue is int autorId ? autorId : 0,
                Ejemplares = ejemplares
            };
        }

        private void Limpiar()
        {
            _seleccionado = null;
            dgLibros.SelectedItem = null;
            txtModo.Text = "Nuevo libro";
            txtTitulo.Clear();
            txtIsbn.Clear();
            cboAutor.SelectedIndex = -1;
            txtEjemplares.Clear();
            btnEliminar.IsEnabled = false;
            btnGuardar.IsEnabled = true;
        }
    }
}
