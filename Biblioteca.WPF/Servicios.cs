using Biblioteca.Datos;
using Biblioteca.Negocio;

namespace Biblioteca.WPF
{
    internal static class Servicios
    {
        public static LibroNegocio CrearLibroNegocio()
        {
            return new LibroNegocio(new LibroDatos(), new AutorDatos());
        }

        public static SocioNegocio CrearSocioNegocio()
        {
            return new SocioNegocio(new SocioDatos());
        }

        public static PrestamoNegocio CrearPrestamoNegocio()
        {
            return new PrestamoNegocio(new SocioDatos(), new LibroDatos(), new PrestamoDatos(), new DetallePrestamoDatos());
        }
    }
}
