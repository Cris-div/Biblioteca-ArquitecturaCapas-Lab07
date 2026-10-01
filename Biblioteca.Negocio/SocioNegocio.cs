using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio
{
    public class SocioNegocio
    {
        private readonly SocioDatos _socios;

        public SocioNegocio() : this(new SocioDatos()) { }
        public SocioNegocio(SocioDatos socios) { _socios = socios ?? throw new ArgumentNullException(nameof(socios)); }

        public Task<List<Socio>> BuscarAsync(string filtro) => _socios.BuscarAsync(filtro);

        public async Task<int> InsertarAsync(Socio socio)
        {
            Validar(socio);
            if (await _socios.ExisteDniAsync(socio.DNI).ConfigureAwait(false))
                throw new ReglaNegocioException("Ya existe un socio registrado con ese DNI.");
            return await _socios.InsertarAsync(socio).ConfigureAwait(false);
        }

        public async Task ActualizarAsync(Socio socio)
        {
            Validar(socio);
            var actual = await _socios.ObtenerPorIdAsync(socio.SocioId).ConfigureAwait(false);
            if (actual == null) throw new ReglaNegocioException("No se encontró el socio que deseas actualizar.");
            if (await _socios.ExisteDniAsync(socio.DNI, socio.SocioId).ConfigureAwait(false))
                throw new ReglaNegocioException("Otro socio ya está registrado con ese DNI.");
            await _socios.ActualizarAsync(socio).ConfigureAwait(false);
        }

        public async Task DarDeBajaAsync(int socioId)
        {
            var actual = await _socios.ObtenerPorIdAsync(socioId).ConfigureAwait(false);
            if (actual == null || !actual.Activo)
                throw new ReglaNegocioException("El socio no existe o ya está dado de baja.");
            if (await _socios.TienePrestamosPendientesAsync(socioId).ConfigureAwait(false))
                throw new ReglaNegocioException("No se puede dar de baja un socio con préstamos pendientes.");
            try
            {
                await _socios.DarDeBajaAsync(socioId).ConfigureAwait(false);
            }
            catch (InvalidOperationException ex)
            {
                throw new ReglaNegocioException(ex.Message);
            }
        }

        private static void Validar(Socio socio)
        {
            if (socio == null) throw new ReglaNegocioException("Debes indicar los datos del socio.");
            if (string.IsNullOrWhiteSpace(socio.DNI)) throw new ReglaNegocioException("El DNI es obligatorio.");
            if (socio.DNI.Trim().Length > 15) throw new ReglaNegocioException("El DNI no puede superar los 15 caracteres.");
            if (string.IsNullOrWhiteSpace(socio.Nombre)) throw new ReglaNegocioException("El nombre del socio es obligatorio.");
            if (!string.IsNullOrWhiteSpace(socio.Email) && socio.Email.Trim().Length > 160)
                throw new ReglaNegocioException("El correo no puede superar los 160 caracteres.");
        }
    }
}
