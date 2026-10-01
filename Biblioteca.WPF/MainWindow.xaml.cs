using System.Windows;
using Biblioteca.WPF.Vistas;

namespace Biblioteca.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Abrir(Window ventana)
        {
            ventana.Owner = this;
            ventana.Show();
        }

        private void BtnLibros_Click(object sender, RoutedEventArgs e)
        {
            Abrir(new LibrosWindow());
        }

        private void BtnSocios_Click(object sender, RoutedEventArgs e)
        {
            Abrir(new SociosWindow());
        }

        private void BtnPrestamo_Click(object sender, RoutedEventArgs e)
        {
            Abrir(new PrestamoWindow());
        }

        private void BtnDevolucion_Click(object sender, RoutedEventArgs e)
        {
            Abrir(new DevolucionWindow());
        }

        private void BtnReporte_Click(object sender, RoutedEventArgs e)
        {
            Abrir(new ReporteWindow());
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
