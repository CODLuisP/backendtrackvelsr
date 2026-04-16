using Microsoft.AspNetCore.Mvc;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Model.Geocerca;

namespace VelsatBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class GeocercaController : ControllerBase
    {
        private readonly IReadOnlyUnitOfWork _readOnlyUow;  // ✅ Para GET
        private readonly IUnitOfWork _uow;

        public GeocercaController(IReadOnlyUnitOfWork readOnlyUow, IUnitOfWork uow)
        {
            _readOnlyUow = readOnlyUow;
            _uow = uow;
        }

        [HttpGet("GetGeocercas")]
        public async Task<IActionResult> GetGeocerca([FromQuery] string usuario)
        {
            try
            {
                var geo = await _readOnlyUow.GeocercaRepository.GetGeocercas(usuario);
                return Ok(geo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las geocercas", error = ex.Message });
            }
        }

        [HttpPost("InsertGeocerca")]
        public async Task<IActionResult> InsertGeocerca([FromBody] Geocerca geocerca)
        {
            try
            {
                if (geocerca == null)
                {
                    return BadRequest(new { message = "La geocerca no puede ser nula" });
                }
                var rowsAffected = await _uow.GeocercaRepository.InsertGeocerca(geocerca);
                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "No se pudo insertar la geocerca" });
                }
                _uow.SaveChanges();
                return Ok(new { message = "Geocerca insertada correctamente", geocerca });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al insertar la geocerca", error = ex.Message });
            }
        }

        [HttpDelete("DeleteGeocerca")]
        public async Task<IActionResult> DeleteGeocerca([FromQuery] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "El id no puede ser nulo o vacío" });
                }
                var rowsAffected = await _uow.GeocercaRepository.DeleteGeocerca(id);
                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Geocerca no encontrada" });
                }
                _uow.SaveChanges();
                return Ok(new { message = "Geocerca eliminada correctamente", id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar la geocerca", error = ex.Message });
            }
        }
    }
}
