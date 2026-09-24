using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Data.Services;

namespace VelsatBackendAPI.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepository { get; }

        IDatosCargainicialService DatosCargainicialService { get; }

        IHistoricosRepository HistoricosRepository { get; }

        IKilometrosRepository KilometrosRepository { get; }

        IServidorRepository ServidorRepository { get; }

        IAlertaRepository AlertaRepository { get; }

        IAdminRepository AdminRepository { get; }

        IGeocercaRepository GeocercaRepository { get; }

        IGeocercasRepository GeocercasRepository { get; }

        IGeocercasVehiculosRepository GeocercasVehiculosRepository { get; }

        void SaveChanges();

    }
}
