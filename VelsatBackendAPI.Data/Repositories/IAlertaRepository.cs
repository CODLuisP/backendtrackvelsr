using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model.AlarmasCorreo;

namespace VelsatBackendAPI.Data.Repositories
{
    public interface IAlertaRepository
    {
        Task<List<RegistroAlarmas>> ObtenerAlertasNoEnviadasAsync();
        Task MarcarComoEnviadasAsync(List<int> ids);
    }
}
