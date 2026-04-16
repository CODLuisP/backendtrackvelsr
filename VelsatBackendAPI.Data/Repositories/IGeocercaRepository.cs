using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model.Geocerca;

namespace VelsatBackendAPI.Data.Repositories
{
    public interface IGeocercaRepository
    {
        Task<IEnumerable<Geocerca>> GetGeocercas(string usuario);

        Task<int> InsertGeocerca(Geocerca geocerca);

        Task<int> DeleteGeocerca(int id);
    }
}
