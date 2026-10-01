using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Biblioteca.Negocio;

namespace Biblioteca.WPF
{
    internal static class Operacion
    {
        public static async Task EjecutarAsync(UIElement contenedor, Func<Task> accion)
        {
            contenedor.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                await accion();
            }
            catch (ReglaNegocioException ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show(ex.Message, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show("Ocurrió un error inesperado: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                contenedor.IsEnabled = true;
            }
        }
    }
}
