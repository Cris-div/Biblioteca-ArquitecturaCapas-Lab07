using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio
{
    public interface IAutorRepositorio
    {
        Task<List<Autor>> ListarActivosAsync();
    }
}
