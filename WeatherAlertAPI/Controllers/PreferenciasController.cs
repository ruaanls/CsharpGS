using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeatherAlertAPI.Models;
using WeatherAlertAPI.Services;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Http;

namespace WeatherAlertAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "v1")]
    [SwaggerTag("Endpoints para gerenciamento de preferências de notificação")]
    public class PreferenciasController : ControllerBase
    {
        private readonly IPreferenciasService _preferenciasService;

        public PreferenciasController(IPreferenciasService preferenciasService)
        {
            _preferenciasService = preferenciasService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PreferenciasNotificacao>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PreferenciasNotificacao>>> GetPreferencias(
            [FromQuery] string? cidade = "",
            [FromQuery] string? estado = "")
        {
            var preferencias = await _preferenciasService.GetPreferenciasAsync(cidade, estado);
            return Ok(preferencias);
        }

        /// <summary>
        /// Obtém uma preferência específica pelo ID
        /// </summary>
        /// <param name="id">ID da preferência</param>
        /// <returns>Detalhes da preferência</returns>
        /// <response code="200">Retorna a preferência encontrada</response>
        /// <response code="404">Preferência não encontrada</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PreferenciasNotificacao), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PreferenciasNotificacao>> GetPreferencia(int id)
        {
            var preferencia = await _preferenciasService.GetPreferenciaByIdAsync(id);
            if (preferencia == null)
                return NotFound();

            return Ok(preferencia);
        }

        /// <summary>
        /// Cria uma nova preferência de notificação
        /// </summary>
        /// <param name="preferencia">Dados da preferência</param>
        /// <returns>Preferência criada</returns>
        /// <response code="201">Retorna a preferência criada</response>
        /// <response code="400">Dados inválidos</response>
        [HttpPost]
        [ProducesResponseType(typeof(PreferenciasNotificacao), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PreferenciasNotificacao>> CreatePreferencia([FromBody] PreferenciasNotificacao preferencia)
        {
            var result = await _preferenciasService.CreatePreferenciaAsync(preferencia);
            return CreatedAtAction(nameof(GetPreferencia), new { id = result.IdPreferencia }, result);
        }

        /// <summary>
        /// Atualiza uma preferência existente
        /// </summary>
        /// <param name="id">ID da preferência</param>
        /// <param name="preferencia">Novos dados da preferência</param>
        /// <returns>Nenhum conteúdo</returns>
        /// <response code="204">Preferência atualizada com sucesso</response>
        /// <response code="400">Dados inválidos</response>
        /// <response code="404">Preferência não encontrada</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePreferencia(int id, [FromBody] PreferenciasNotificacao preferencia)
        {
            if (id != preferencia.IdPreferencia)
                return BadRequest("ID na URL não corresponde ao ID no corpo da requisição");

            await _preferenciasService.UpdatePreferenciaAsync(preferencia);
            return NoContent();
        }

        /// <summary>
        /// Exclui uma preferência
        /// </summary>
        /// <param name="id">ID da preferência</param>
        /// <returns>Nenhum conteúdo</returns>
        /// <response code="204">Preferência excluída com sucesso</response>
        /// <response code="404">Preferência não encontrada</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePreferencia(int id)
        {
            await _preferenciasService.DeletePreferenciaAsync(id);
            return NoContent();
        }
    }
}
