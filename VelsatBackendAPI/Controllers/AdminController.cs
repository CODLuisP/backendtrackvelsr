using Microsoft.AspNetCore.Mvc;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Model.Administracion;
using VelsatBackendAPI.Model.Documentacion;

namespace VelsatBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IReadOnlyUnitOfWork _readOnlyUow;  // ✅ Para GET
        private readonly IUnitOfWork _uow;

        public AdminController(IReadOnlyUnitOfWork readOnlyUow, IUnitOfWork uow)
        {
            _readOnlyUow = readOnlyUow;
            _uow = uow;
        }

        [HttpGet("Usuarios")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _readOnlyUow.AdminRepository.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los usuarios", error = ex.Message });
            }
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] Usuarioadmin usuario, [FromQuery] string actor)
        {
            try
            {
                if (usuario == null)
                {
                    return BadRequest(new { message = "El usuario no puede ser nulo" });
                }

                var rowsAffected = await _uow.AdminRepository.UpdateUser(usuario);

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                await _uow.AdminRepository.RegistrarAuditoria(actor, "Usuarios", "Actualizar", usuario.AccountID,
                    $"Se actualizó el usuario {usuario.AccountID}");

                _uow.SaveChanges();

                return Ok(new { message = "Usuario actualizado correctamente", rowsAffected });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el usuario", error = ex.Message });
            }
        }

        [HttpDelete("DeleteUsuario/{accountID}")]
        public async Task<IActionResult> DeleteUser(string accountID, [FromQuery] string actor)
        {
            try
            {
                if (string.IsNullOrEmpty(accountID))
                {
                    return BadRequest(new { message = "El accountID no puede ser nulo o vacío" });
                }

                var rowsAffected = await _uow.AdminRepository.DeleteUser(accountID);

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                await _uow.AdminRepository.RegistrarAuditoria(actor, "Usuarios", "Eliminar", accountID,
                    $"Se eliminó el usuario {accountID}");

                _uow.SaveChanges();

                return Ok(new { message = "Usuario eliminado correctamente", accountID });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el usuario", error = ex.Message });
            }
        }

        [HttpPost("InsertUsuario")]
        public async Task<IActionResult> InsertUser([FromBody] Usuarioadmin usuario, [FromQuery] string actor)
        {
            try
            {
                if (usuario == null)
                {
                    return BadRequest(new { message = "El usuario no puede ser nulo" });
                }

                if (string.IsNullOrEmpty(usuario.AccountID) || string.IsNullOrEmpty(usuario.Password))
                {
                    return BadRequest(new { message = "AccountID y Password son obligatorios" });
                }

                var rowsAffected = await _uow.AdminRepository.InsertUser(usuario);

                if (rowsAffected == 0)
                {
                    return StatusCode(500, new { message = "No se pudo insertar el usuario" });
                }

                await _uow.AdminRepository.RegistrarAuditoria(actor, "Usuarios", "Crear", usuario.AccountID,
                    $"Se creó el usuario {usuario.AccountID}");

                _uow.SaveChanges();

                return Ok(new { message = "Usuario creado correctamente", accountID = usuario.AccountID });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el usuario", error = ex.Message });
            }
        }

        [HttpGet("SubUsuarios")]
        public async Task<IActionResult> GetSubUsers()
        {
            try
            {
                var users = await _readOnlyUow.AdminRepository.GetSubUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los subusuarios", error = ex.Message });
            }
        }

        [HttpPost("InsertDeviceUser")]
        public async Task<IActionResult> InsertSubUser([FromBody] Deviceuser usuario, [FromQuery] string actor)
        {
            try
            {
                if (usuario == null)
                {
                    return BadRequest(new { message = "El device user no puede ser nulo" });
                }

                var rowsAffected = await _uow.AdminRepository.InsertSubUser(usuario);

                if (rowsAffected == 0)
                {
                    return StatusCode(500, new { message = "No se pudo insertar el device user" });
                }

                await _uow.AdminRepository.RegistrarAuditoria(actor, "SubUsuarios", "Crear", usuario.UserId,
                    $"Se creó el subusuario {usuario.UserId} sobre el dispositivo {usuario.DeviceID}");

                _uow.SaveChanges();

                return Ok(new { message = "Device user creado correctamente", id = usuario.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el device user", error = ex.Message });
            }
        }

        [HttpPut("UpdateDeviceUser")]
        public async Task<IActionResult> UpdateSubUser([FromBody] Deviceuser usuario, [FromQuery] string actor)
        {
            try
            {
                if (usuario == null)
                {
                    return BadRequest(new { message = "El device user no puede ser nulo" });
                }

                var rowsAffected = await _uow.AdminRepository.UpdateSubUser(usuario);

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Device user no encontrado" });
                }

                await _uow.AdminRepository.RegistrarAuditoria(actor, "SubUsuarios", "Actualizar", usuario.UserId,
                    $"Se actualizó el subusuario {usuario.UserId}");

                _uow.SaveChanges();

                return Ok(new { message = "Device user actualizado correctamente", rowsAffected });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el device user", error = ex.Message });
            }
        }

        [HttpDelete("DeleteDeviceUser/{id}")]
        public async Task<IActionResult> DeleteSubUser(string id, [FromQuery] string actor)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { message = "El id no puede ser nulo o vacío" });
                }

                var rowsAffected = await _uow.AdminRepository.DeleteSubUser(id);

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Device user no encontrado" });
                }

                await _uow.AdminRepository.RegistrarAuditoria(actor, "SubUsuarios", "Eliminar", id,
                    $"Se eliminó el subusuario con id {id}");

                _uow.SaveChanges();

                return Ok(new { message = "Device user eliminado correctamente", id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el device user", error = ex.Message });
            }
        }

        [HttpGet("GetDevices")]
        public async Task<IActionResult> GetDevices()
        {
            try
            {
                var devices = await _readOnlyUow.AdminRepository.GetDevices();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las unidades", error = ex.Message });
            }
        }

        [HttpPut("UpdateDevice")]
        public async Task<IActionResult> UpdateDevice([FromBody] DeviceAdmin device, string oldDeviceID, string oldAccountID, [FromQuery] string actor)
        {
            try
            {
                var resultado = await _uow.AdminRepository.UpdateDevice(device, oldDeviceID, oldAccountID);

                if (resultado == 0)
                {
                    return NotFound(new { message = "Dispositivo no encontrado" });
                }

                await _uow.AdminRepository.RegistrarAuditoria(actor, "Unidades", "Actualizar", device.DeviceID,
                    $"Se actualizó el dispositivo {oldDeviceID} (cuenta {oldAccountID})");

                _uow.SaveChanges();

                return Ok(new { message = "Dispositivo actualizado correctamente", rowsAffected = resultado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el dispositivo", error = ex.Message });
            }
        }

        [HttpPost("InsertDevice")]
        public async Task<IActionResult> InsertDevice([FromBody] DeviceAdmin device, [FromQuery] string actor)
        {
            try
            {
                var resultado = await _uow.AdminRepository.InsertDevice(device);

                if (resultado == 0)
                {
                    return BadRequest(new { message = "No se pudo crear el dispositivo" });
                }

                await _uow.AdminRepository.RegistrarAuditoria(actor, "Unidades", "Crear", device.DeviceID,
                    $"Se creó el dispositivo {device.DeviceID} para la cuenta {device.AccountID}");

                _uow.SaveChanges();

                return Ok(new { message = "Dispositivo creado correctamente", rowsAffected = resultado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el dispositivo", error = ex.Message });
            }
        }

        [HttpDelete("DeleteDevice/{deviceID}/{accountID}")]
        public async Task<IActionResult> DeleteDevice(string deviceID, string accountID, [FromQuery] string actor)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceID) || string.IsNullOrEmpty(accountID))
                {
                    return BadRequest(new { message = "El deviceID y accountID no pueden ser nulos o vacíos" });
                }

                var rowsAffected = await _uow.AdminRepository.DeleteDevice(deviceID, accountID);

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Dispositivo no encontrado" });
                }

                await _uow.AdminRepository.RegistrarAuditoria(actor, "Unidades", "Eliminar", deviceID,
                    $"Se eliminó el dispositivo {deviceID} de la cuenta {accountID}");

                _uow.SaveChanges();

                return Ok(new { message = "Dispositivo eliminado correctamente", deviceID, accountID });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el dispositivo", error = ex.Message });
            }
        }

        [HttpGet("GetDevicesConex")]
        public async Task<IActionResult> GetConexDesconex()
        {
            try
            {
                var devices = await _readOnlyUow.AdminRepository.GetConexDesconex();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las unidades", error = ex.Message });
            }
        }

        [HttpGet("GetUnidadesGoldcar")]
        public async Task<IActionResult> GetUnidadesGoldcar()
        {
            try
            {
                var devices = await _readOnlyUow.AdminRepository.GetUnidadesGoldcar();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las unidades Goldcar", error = ex.Message });
            }
        }

        [HttpPut("HabilitarGoldcar")]
        public async Task<IActionResult> HabilitarGoldcar(string accountID, string deviceID, char valor, [FromQuery] string actor)
        {
            try
            {
                if (string.IsNullOrEmpty(accountID) || string.IsNullOrEmpty(deviceID))
                {
                    return BadRequest(new { message = "El accountID y deviceID no pueden ser nulos o vacíos" });
                }

                var rowsAffected = await _uow.AdminRepository.HabilitarGoldcar(accountID, deviceID, valor);

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Dispositivo no encontrado" });
                }

                await _uow.AdminRepository.RegistrarAuditoria(actor, "Goldcar", valor == '1' ? "Habilitar" : "Deshabilitar", deviceID,
                    $"Se {(valor == '1' ? "habilitó" : "deshabilitó")} Goldcar para el dispositivo {deviceID} (cuenta {accountID})");

                _uow.SaveChanges();

                return Ok(new { message = "Sutran actualizado correctamente", accountID, deviceID, valor });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar Goldcar", error = ex.Message });
            }
        }

        [HttpGet("GetAuditoriaGoldcar")]
        public async Task<IActionResult> GetUltimosRegistrosAuditoriaGoldcar(string accountID, string deviceID)
        {
            try
            {
                if (string.IsNullOrEmpty(accountID) || string.IsNullOrEmpty(deviceID))
                {
                    return BadRequest(new { message = "El accountID y deviceID no pueden ser nulos o vacíos" });
                }

                var registros = await _readOnlyUow.AdminRepository.GetUltimosRegistrosAuditoriaGoldcar(accountID, deviceID);
                return Ok(registros);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la auditoría Goldcar", error = ex.Message });
            }
        }

        [HttpGet("GetUnidadesSutran")]
        public async Task<IActionResult> GetUnidadesSutran()
        {
            try
            {
                var devices = await _readOnlyUow.AdminRepository.GetUnidadesSutran();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las unidades Sutran", error = ex.Message });
            }
        }

        [HttpPut("HabilitarSutran")]
        public async Task<IActionResult> HabilitarSutran(string accountID, string deviceID, char valor, [FromQuery] string actor)
        {
            try
            {
                if (string.IsNullOrEmpty(accountID) || string.IsNullOrEmpty(deviceID))
                {
                    return BadRequest(new { message = "El accountID y deviceID no pueden ser nulos o vacíos" });
                }

                var rowsAffected = await _uow.AdminRepository.HabilitarSutran(accountID, deviceID, valor);

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Dispositivo no encontrado" });
                }

                await _uow.AdminRepository.RegistrarAuditoria(actor, "Sutran", valor == '1' ? "Habilitar" : "Deshabilitar", deviceID,
                    $"Se {(valor == '1' ? "habilitó" : "deshabilitó")} Sutran para el dispositivo {deviceID} (cuenta {accountID})");

                _uow.SaveChanges();

                return Ok(new { message = "Sutran actualizado correctamente", accountID, deviceID, valor });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar Sutran", error = ex.Message });
            }
        }

        [HttpGet("GetAuditoriaSutran")]
        public async Task<IActionResult> GetUltimosRegistrosAuditoriaSutran(string accountID, string deviceID)
        {
            try
            {
                if (string.IsNullOrEmpty(accountID) || string.IsNullOrEmpty(deviceID))
                {
                    return BadRequest(new { message = "El accountID y deviceID no pueden ser nulos o vacíos" });
                }

                var registros = await _readOnlyUow.AdminRepository.GetUltimosRegistrosAuditoriaSutran(accountID, deviceID);
                return Ok(registros);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la auditoría UTRAN", error = ex.Message });
            }
        }

        [HttpGet("GetUnidadesOsinergmin")]
        public async Task<IActionResult> GetUnidadesOsinergmin()
        {
            try
            {
                var devices = await _readOnlyUow.AdminRepository.GetUnidadesOsinergmin();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las unidades Osinergmin", error = ex.Message });
            }
        }

        [HttpPut("HabilitarOsinergmin")]
        public async Task<IActionResult> HabilitarOsinergmin(string accountID, string deviceID, char valor, [FromQuery] string actor)
        {
            try
            {
                if (string.IsNullOrEmpty(accountID) || string.IsNullOrEmpty(deviceID))
                {
                    return BadRequest(new { message = "El accountID y deviceID no pueden ser nulos o vacíos" });
                }

                var rowsAffected = await _uow.AdminRepository.HabilitarOsinergmin(accountID, deviceID, valor);

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Dispositivo no encontrado" });
                }

                await _uow.AdminRepository.RegistrarAuditoria(actor, "Osinergmin", valor == '1' ? "Habilitar" : "Deshabilitar", deviceID,
                    $"Se {(valor == '1' ? "habilitó" : "deshabilitó")} Osinergmin para el dispositivo {deviceID} (cuenta {accountID})");

                _uow.SaveChanges();

                return Ok(new { message = "Osinergmin actualizado correctamente", accountID, deviceID, valor });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar Osinergmin", error = ex.Message });
            }
        }

        [HttpGet("GetAuditoriaGeneral")]
        public async Task<IActionResult> GetAuditoriaGeneral(int limit = 200, string modulo = null, string usuario = null)
        {
            try
            {
                var registros = await _readOnlyUow.AdminRepository.GetAuditoriaGeneral(limit, modulo, usuario);
                return Ok(registros);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la auditoría general", error = ex.Message });
            }
        }

        [HttpGet("GetAuditoriaOsinergmin")]
        public async Task<IActionResult> GetUltimosRegistrosAuditoriaOsinergmin(string accountID, string deviceID)
        {
            try
            {
                if (string.IsNullOrEmpty(accountID) || string.IsNullOrEmpty(deviceID))
                {
                    return BadRequest(new { message = "El accountID y deviceID no pueden ser nulos o vacíos" });
                }

                var registros = await _readOnlyUow.AdminRepository.GetUltimosRegistrosAuditoriaOsinergmin(accountID, deviceID);
                return Ok(registros);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la auditoría Osinergmin", error = ex.Message });
            }
        }

        //DOCUMENTACIÓN
        //----------------------------------UNIDAD--------------------------------------------------//

        [HttpGet("GetDocumento")]
        public async Task<IActionResult> GetDocumento([FromQuery] string accountID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El AccountID es requerido"
                    });
                }

                var documentos = await _readOnlyUow.AdminRepository.GetDocumento(accountID);

                if (documentos == null || !documentos.Any())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No se encontraron documentos para la cuenta {accountID}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = documentos,
                    count = documentos.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        [HttpPost("CreateDocumento")]
        public async Task<IActionResult> CreateDocumento([FromBody] Documento documento)
        {
            try
            {
                if (documento == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Los datos del documento son requeridos"
                    });
                }

                var id = await _uow.AdminRepository.CreateDocumento(documento);
                _uow.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Documento creado exitosamente",
                    id = id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("DeleteDocumento")]
        public async Task<IActionResult> DeleteUnidad([FromQuery] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID inválido"
                    });
                }

                var resultado = await _uow.AdminRepository.DeleteDocumento(id);
                _uow.SaveChanges();

                if (!resultado)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No se encontró el documento con ID {id}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Documento eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        [HttpGet("DocumentosPorVencer")]
        public async Task<IActionResult> DocumentosPorVencer([FromQuery] string accountID)
        {
            try
            {
                var documentos = await _readOnlyUow.AdminRepository.DocumentosPorVencer(accountID);

                if (documentos == null || !documentos.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No se encontraron documentos próximos a vencer"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = documentos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }       
    }
}
