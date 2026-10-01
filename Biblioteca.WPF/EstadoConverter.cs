using System;
using System.Globalization;
using System.Windows.Data;

namespace Biblioteca.WPF
{
    public class EstadoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool activo && activo ? "Activo" : "Inactivo";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
