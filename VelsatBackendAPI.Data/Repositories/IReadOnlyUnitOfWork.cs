using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Data.Services;

namespace VelsatBackendAPI.Data.Repositories
{
    /// <summary>
    /// Interfaz para operaciones de solo lectura (sin transacciones)
    /// </summary>
    public interface IReadOnlyUnitOfWork : IDisposable
    {
        IDatosCargainicialService DatosCargainicialService { get; }

        IServidorRepository ServidorRepository { get; }

        IHistoricosRepository HistoricosRepository { get; }

        IKilometrosRepository KilometrosRepository { get; }

        IUserRepository UserRepository { get; }

        IAdminRepository AdminRepository { get; }

        IGeocercaRepository GeocercaRepository { get; }

        IGeocercasRepository GeocercasRepository { get; }

        IGeocercasVehiculosRepository GeocercasVehiculosRepository { get; }

    }
}