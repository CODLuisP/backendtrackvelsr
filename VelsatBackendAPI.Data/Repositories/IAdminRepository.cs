using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model.Administracion;
using VelsatBackendAPI.Model.Documentacion;

namespace VelsatBackendAPI.Data.Repositories
{
    public interface IAdminRepository
    {
        Task<IEnumerable<Usuarioadmin>> GetAllUsers();

        Task<int> UpdateUser(Usuarioadmin usuario);

        Task<int> DeleteUser(string accountID);

        Task<int> InsertUser(Usuarioadmin usuario);

        Task<IEnumerable<Deviceuser>> GetSubUsers();

        Task<int> UpdateSubUser(Deviceuser usuario);

        Task<int> DeleteSubUser(string id);

        Task<int> InsertSubUser(Deviceuser usuario);

        Task<IEnumerable<DeviceAdmin>> GetDevices();

        Task<int> UpdateDevice(DeviceAdmin device, string oldDeviceID, string oldAccountID);

        Task<int> InsertDevice(DeviceAdmin device);

        Task<int> DeleteDevice(string deviceID, string accountID);

        Task<IEnumerable<ConexDevice>> GetConexDesconex();

        Task<IEnumerable<Auditoria>> GetUltimosRegistrosAuditoriaGoldcar(string accountID, string deviceID);

        Task<int> HabilitarGoldcar(string accountID, string deviceID, char valor);

        Task<IEnumerable<DeviceAuditoria>> GetUnidadesGoldcar();

        Task<IEnumerable<AuditoriaSutran>> GetUltimosRegistrosAuditoriaSutran(string accountID, string deviceID);

        Task<int> HabilitarSutran(string accountID, string deviceID, char valor);

        Task<IEnumerable<DeviceSutran>> GetUnidadesSutran();


        //DOCUMENTACIÓN
        //----------------------------------UNIDAD--------------------------------------------------//
        Task<List<Documento>> GetDocumento(string accountID);
        Task<int> CreateDocumento(Documento documento);
        Task<bool> DeleteDocumento(int id);
        Task<List<Documento>> DocumentosPorVencer(string accountID);
    }
}
