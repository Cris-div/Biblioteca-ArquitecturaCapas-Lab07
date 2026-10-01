using System;
using System.Configuration;
using System.Data.SqlClient;

namespace Biblioteca.Datos
{
    internal static class Conexion
    {
        public static SqlConnection Crear()
        {
            var configuracion = ConfigurationManager.ConnectionStrings["BibliotecaDB"];
            if (configuracion == null || string.IsNullOrWhiteSpace(configuracion.ConnectionString))
                throw new ConfigurationErrorsException(
                    "No se encontró la cadena de conexión 'BibliotecaDB' en el App.config de la aplicación.");

            return new SqlConnection(configuracion.ConnectionString);
        }
    }
}
