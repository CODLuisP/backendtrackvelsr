using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using VelsatBackendAPI.Model.Administracion;
using VelsatBackendAPI.Model.Documentacion;

namespace VelsatBackendAPI.Data.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly IDbConnection _defaultConnection; IDbTransaction _defaultTransaction; IDbConnection _thridconnection; IDbTransaction _thridTransaction;

        public AdminRepository(IDbConnection defaultconnection, IDbTransaction defaulttransaction, IDbConnection thridconnection, IDbTransaction thridtransaction)
        {
            _defaultConnection = defaultconnection;
            _defaultTransaction = defaulttransaction;
            _thridconnection = thridconnection;
            _thridTransaction = thridtransaction;
        }

        public async Task<IEnumerable<Usuarioadmin>> GetAllUsers()
        {
            var sql = @"SELECT accountID, password, contactPhone, contactEmail, description, creationTime, isActive, ruc from usuarios WHERE isActive = 1";

            var resultado = await _defaultConnection.QueryAsync<Usuarioadmin>(sql, transaction: _defaultTransaction);

            return resultado;
        }

        public async Task<int> UpdateUser(Usuarioadmin usuario)
        {
            var sql = @"UPDATE usuarios SET password = @Password, contactPhone = @ContactPhone, contactEmail = @ContactEmail, description = @Description, ruc = @Ruc WHERE accountID = @AccountID";

            var resultado = await _defaultConnection.ExecuteAsync(sql, usuario, transaction: _defaultTransaction);

            return resultado;
        }

        public async Task<int> DeleteUser(string accountID)
        {
            var sql = @"UPDATE usuarios SET isActive = 0 WHERE accountID = @AccountID";

            var parametros = new { AccountID = accountID };

            var resultado = await _defaultConnection.ExecuteAsync(sql, parametros, transaction: _defaultTransaction);
            return resultado;
        }

        public async Task<int> InsertUser(Usuarioadmin usuario)
        {
            // Obtener timestamp Unix en hora de Perú (UTC-5)
            var peruTime = DateTime.UtcNow.AddHours(-5);
            var unixTimestamp = ((DateTimeOffset)peruTime).ToUnixTimeSeconds();

            // Asignar valores por defecto
            usuario.CreationTime = (int)unixTimestamp;
            usuario.IsActive = true; // Dapper lo convertirá a 1 en MySQL

            // 1. Insertar usuario
            var sqlUsuario = @"INSERT INTO usuarios 
                (accountID, userID, password, contactPhone, contactEmail, description, creationTime, isActive, ruc) 
                VALUES 
                (@AccountID, 'admin', @Password, @ContactPhone, @ContactEmail, @Description, @CreationTime, @IsActive, @Ruc)";

            var resultado = await _defaultConnection.ExecuteAsync(sqlUsuario, usuario, transaction: _defaultTransaction);

            // 2. Insertar en serverprueba
            var sqlServerPrueba = @"INSERT INTO serverprueba (loginusu, servidor, tipo) 
                            VALUES (@AccountID, 'https://sub.velsat.pe:2096', 'n')";

            await _thridconnection.ExecuteAsync(sqlServerPrueba, new { AccountID = usuario.AccountID }, transaction: _thridTransaction);

            // 3. Insertar en servermobile
            var sqlServerMobile = @"INSERT INTO servermobile (loginusu, servidor, tipo) 
                            VALUES (@AccountID, 'https://sub.velsat.pe:2087', 'n')";

            await _thridconnection.ExecuteAsync(sqlServerMobile, new { AccountID = usuario.AccountID }, transaction: _thridTransaction);

            return resultado;
        }

        public async Task<IEnumerable<Deviceuser>> GetSubUsers()
        {
            var sql = @"SELECT id, UserId, DeviceName, Status, DeviceID from deviceuser WHERE status = '1'";

            var resultado = await _defaultConnection.QueryAsync<Deviceuser>(sql, transaction: _defaultTransaction);

            return resultado;
        }

        public async Task<int> InsertSubUser(Deviceuser usuario)
        {
            var sql = @"INSERT INTO deviceuser (id, UserId, DeviceName, Status, DeviceID) VALUES (@Id, @UserId, @DeviceName, '1', @DeviceID)";

            var resultado = await _defaultConnection.ExecuteAsync(sql, usuario, transaction: _defaultTransaction);
            return resultado;
        }

        public async Task<int> UpdateSubUser(Deviceuser usuario)
        {
            var sql = @"UPDATE deviceuser SET UserID = @UserId, DeviceName = @DeviceName, deviceID = @DeviceID WHERE id = @Id";

            var resultado = await _defaultConnection.ExecuteAsync(sql, usuario, transaction: _defaultTransaction);
            return resultado;
        }

        public async Task<int> DeleteSubUser(string id)
        {
            var sql = @"UPDATE deviceuser SET status = '0' WHERE id = @Id";

            var parametros = new { Id = id };

            var resultado = await _defaultConnection.ExecuteAsync(sql, parametros, transaction: _defaultTransaction);
            return resultado;
        }

        public async Task<IEnumerable<DeviceAdmin>> GetDevices()
        {
            var sql = @"SELECT deviceID, accountID, equipmentType, uniqueID, deviceCode, simPhoneNumber, imeiNumber, isActive from device order by accountID";

            var resultado = await _defaultConnection.QueryAsync<DeviceAdmin>(sql, transaction: _defaultTransaction);

            return resultado;
        }

        public async Task<int> UpdateDevice(DeviceAdmin device, string oldDeviceID, string oldAccountID)
        {
            var sql = @"UPDATE device SET deviceID = @DeviceID, accountID = @AccountID, equipmentType = @EquipmentType, uniqueID = @UniqueID, deviceCode = @DeviceCode, simPhoneNumber = @SimPhoneNumber, imeiNumber = @ImeiNumber WHERE deviceID = @OldDeviceID AND accountID = @OldAccountID";

            var resultado = await _defaultConnection.ExecuteAsync(sql, new
            {
                device.DeviceID,
                device.AccountID,
                device.EquipmentType,
                device.UniqueID,
                device.DeviceCode,
                device.SimPhoneNumber,
                device.ImeiNumber,
                OldDeviceID = oldDeviceID,
                OldAccountID = oldAccountID
            }, transaction: _defaultTransaction);

            return resultado;
        }

        public async Task<int> InsertDevice(DeviceAdmin device)
        {
            var sql = @"INSERT INTO device (deviceID, accountID, equipmentType, uniqueID, deviceCode, simPhoneNumber, imeiNumber, isActive) 
                VALUES (@DeviceID, @AccountID, @EquipmentType, @UniqueID, @DeviceCode, @SimPhoneNumber, @ImeiNumber, '1')";

            var resultado = await _defaultConnection.ExecuteAsync(sql, device, transaction: _defaultTransaction);
            return resultado;
        }

        public async Task<IEnumerable<ConexDevice>> GetConexDesconex()
        {
            var sql = @"SELECT deviceID, accountID, lastValidSpeed, lastGPSTimestamp, deviceCode, imeiNumber, lastValidLatitude, lastValidLongitude FROM device ORDER BY accountID";

            var resultado = await _defaultConnection.QueryAsync<ConexDevice>(sql, transaction: _defaultTransaction);

            return resultado;
        }

        public async Task<int> DeleteDevice(string deviceID, string accountID)
        {
            var sql = @"DELETE FROM device WHERE deviceID = @DeviceID AND accountID = @AccountID";

            var resultado = await _defaultConnection.ExecuteAsync(sql,
                new { DeviceID = deviceID, AccountID = accountID },
                transaction: _defaultTransaction);

            return resultado;
        }

        public async Task<IEnumerable<DeviceAuditoria>> GetUnidadesGoldcar()
        {
            var sql = @"SELECT accountID, deviceID FROM device WHERE godlcar = '1'";

            var resultado = await _defaultConnection.QueryAsync<DeviceAuditoria>(sql, transaction: _defaultTransaction);

            return resultado;
        }

        public async Task<int> HabilitarGoldcarn(string accountID, string deviceID, char valor)
        {
            var sql = @"UPDATE device SET goldcar = @Valor WHERE accountID = @AccountID AND deviceID = @DeviceID";

            var resultado = await _defaultConnection.ExecuteAsync(sql,
                new { Valor = valor, AccountID = accountID, DeviceID = deviceID },
                transaction: _defaultTransaction);

            return resultado;
        }

        public async Task<IEnumerable<Auditoria>> GetUltimosRegistrosAuditoriaGoldcar(string accountID, string deviceID)
        {
            var sql = @"SELECT id, accountID, deviceID, fecharegistro, lastenvio, lastrespuesta
                        FROM auditoriagoldcar
                        WHERE accountID = @AccountID AND deviceID = @DeviceID
                        ORDER BY fecharegistro DESC
                        LIMIT 5";

            var resultado = await _defaultConnection.QueryAsync<Auditoria>(sql,
                new { AccountID = accountID, DeviceID = deviceID },
                transaction: _defaultTransaction);

            return resultado;
        }

        //----------------------------------UNIDAD--------------------------------------------------//
        public async Task<List<Documento>> GetDocumento(string accountID)
        {
            const string sql = @"SELECT * FROM documentos WHERE AccountID = @AccountID ORDER BY Fecha_vencimiento DESC";
            try
            {
                var documentos = await _defaultConnection.QueryAsync<Documento>(sql, new { AccountID = accountID }, transaction: _defaultTransaction);
                return documentos.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener documentos de AccountID");
                throw;
            }
        }

        public async Task<int> CreateDocumento(Documento documento)
        {
            const string sql = @"INSERT INTO documentos (accountID, deviceID, nombre_documento, tipo_documento, archivo_url, fecha_vencimiento, observaciones) 
                                VALUES (@AccountID, @DeviceID, @Nombre_documento, @Tipo_documento, @Archivo_url, @Fecha_vencimiento, @Observaciones); SELECT LAST_INSERT_ID();";
            try
            {
                var id = await _defaultConnection.ExecuteScalarAsync<int>(sql, new
                {
                    documento.AccountID,
                    documento.DeviceID,
                    documento.Nombre_documento,
                    documento.Tipo_documento,
                    documento.Archivo_url,
                    documento.Fecha_vencimiento,
                    documento.Observaciones,
                }, transaction: _defaultTransaction);
                return id;
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                Console.WriteLine("Error de duplicado al insertar en docunidad");
                throw new Exception("El documento de unidad ya existe en el sistema", ex);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error de MySQL al insertar en docunidad");
                throw new Exception($"Error de base de datos MySQL: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al crear documento de unidad");
                throw;
            }
        }

        public async Task<bool> DeleteDocumento(int id)
        {
            const string sql = @"DELETE FROM documentos WHERE Id = @Id";
            try
            {
                var affectedRows = await _defaultConnection.ExecuteAsync(sql, new { Id = id }, transaction: _defaultTransaction);
                return affectedRows > 0;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error de MySQL al eliminar en documentos");
                throw new Exception($"Error de base de datos MySQL: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al eliminar documentos");
                throw;
            }
        }

        public async Task<List<Documento>> DocumentosPorVencer(string accountID)
        {
            var fechaActual = DateTime.UtcNow.AddHours(-5).Date;
            var fechaLimite = fechaActual.AddDays(30);

            const string sql = @"SELECT * FROM documentos WHERE Fecha_vencimiento IS NOT NULL AND Fecha_vencimiento <= @FechaLimite AND accountID = @AccountID ORDER BY Fecha_vencimiento ASC";
            try
            {
                var documentos = await _defaultConnection.QueryAsync<Documento>(sql, new
                {
                    FechaLimite = fechaLimite,
                    AccountID = accountID
                }, transaction: _defaultTransaction);
                return documentos.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener documentos próximos a vencer");
                return new List<Documento>();
            }
        }
    }
}