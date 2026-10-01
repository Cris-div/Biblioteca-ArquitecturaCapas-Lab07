# Integración del backend con Biblioteca.WPF

Los cuatro proyectos están en .NET Framework 4.8. Las referencias aplican inversión de dependencias:

- `Biblioteca.Entidades`: sin referencias a otros proyectos.
- `Biblioteca.Negocio`: referencia solo a `Biblioteca.Entidades`. Declara las interfaces `ILibroRepositorio`, `IAutorRepositorio`, `ISocioRepositorio`, `IPrestamoRepositorio` e `IDetallePrestamoRepositorio`, y las recibe por constructor.
- `Biblioteca.Datos`: referencia a `Biblioteca.Entidades` y `Biblioteca.Negocio`; sus clases implementan esas interfaces con ADO.NET.
- `Biblioteca.WPF` (proyecto de inicio): referencia a los tres. Solo `Servicios.cs` crea las implementaciones de Datos y las inyecta en Negocio; las ventanas trabajan únicamente con Negocio y Entidades y no usan `System.Data.SqlClient`.

En el `App.config` de `Biblioteca.WPF`, agrega la cadena de conexión. Datos la lee con `ConfigurationManager` desde la aplicación de inicio:

```xml
<configuration>
  <connectionStrings>
    <add name="BibliotecaDB"
         connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BibliotecaDB;Integrated Security=True;"
         providerName="System.Data.SqlClient" />
  </connectionStrings>
</configuration>
```

Ejecuta primero `Database/01_Schema.sql` y luego `Database/02_Seed.sql` en SQL Server/LocalDB. El nombre `BibliotecaDB` en la configuración debe coincidir con el nombre usado por `Biblioteca.Datos/Conexion.cs`.

Las llamadas desde los eventos WPF usan `async/await` de punta a punta, por ejemplo `private async void BtnBuscar_Click(...)` con `await _negocio.BuscarAsync(filtro)`. No se usa `.Result` ni `.Wait()`.

## Ventanas WPF

- `MainWindow`: menú principal.
- `Vistas/LibrosWindow`: mantenimiento de libros con nombre del autor; insertar, actualizar, baja lógica y búsqueda por título o autor.
- `Vistas/SociosWindow`: mantenimiento de socios; insertar, actualizar, baja lógica y búsqueda por nombre o DNI.
- `Vistas/PrestamoWindow`: selección de socio y de uno o varios libros, con fecha límite.
- `Vistas/DevolucionWindow`: devolución de un libro pendiente y multa calculada.
- `Vistas/ReporteWindow`: préstamos por intervalo de fechas (INNER JOIN entre Prestamos, DetallePrestamo, Libros y Socios).

Las capturas de las vistas y de las referencias entre proyectos están en la carpeta `Capturas`.

## Operaciones disponibles

- `LibroNegocio`: búsqueda por título o autor, listado de autores activos para el selector, alta, actualización y baja lógica; valida título, ISBN, autor, ejemplares, ISBN duplicado y préstamos pendientes antes de la baja.
- `SocioNegocio`: búsqueda por nombre o DNI, alta, actualización y baja lógica; valida datos, DNI duplicado y préstamos pendientes antes de la baja.
- `PrestamoNegocio`: registra varios libros de forma transaccional, lista todas las pendientes o las de un socio, registra devoluciones con multa de S/ 1.50 por día de retraso e informa préstamos por intervalo de fechas. El registro de pendientes incluye `PrestamoId` y `LibroId` para que WPF pueda enviar una devolución.
- `AutorDatos`: lista autores activos para el selector de autor del formulario de libros.

El alta del préstamo, sus detalles y la reducción de stock se confirman o revierten juntos. La devolución, el registro de fecha, la reposición de stock y el estado del préstamo también se actualizan en una sola transacción.
