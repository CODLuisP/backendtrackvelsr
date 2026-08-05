using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VelsatBackendAPI.Model.AlarmasCorreo;

namespace VelsatBackendAPI.Data.Repositories
{
    public class AlertaRepository : IAlertaRepository
    {
        private readonly IDbConnection _defaultConnection;
        private readonly IDbTransaction _defaultTransaction;

        public AlertaRepository(IDbConnection defaultConnection, IDbTransaction defaultTransaction)
        {
            _defaultConnection = defaultConnection;
            _defaultTransaction = defaultTransaction;
        }

        public async Task<List<RegistroAlarmas>> ObtenerAlertasNoEnviadasAsync()
        {
            const string sql = @"
                SELECT
                    id         AS Codigo,
                    accountID  AS AccountID,
                    deviceID   AS DeviceID,
                    serverTime AS Timestamp,
                    eventType  AS EventType,
                    alarmType  AS AlarmType,
                    latitude   AS Latitude,
                    longitude  AS Longitude,
                    isEnviado  AS IsEnviado
                FROM deviceevent
                WHERE isEnviado = 0
                  AND eventType = 'alarm'
                  AND alarmType = 'lowBattery'
                  AND accountID = 'speedmontalvo'";

            var result = await _defaultConnection.QueryAsync<RegistroAlarmas>(
                sql,
                transaction: _defaultTransaction);
            return result.ToList();
        }

        public async Task MarcarComoEnviadasAsync(List<int> ids)
        {
            if (ids == null || !ids.Any())
                return;

            const string sql = "UPDATE deviceevent SET isEnviado = 1 WHERE id IN @Ids";
            await _defaultConnection.ExecuteAsync(
                sql,
                new { Ids = ids },
                transaction: _defaultTransaction);
        }
    }
}