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
    [SwaggerTag("Gerenciamento de alertas de temperatura")]
    public class AlertaController : ControllerBase
    {
        private readonly IAlertaService _alertaService;
        private readonly IWeatherService _weatherService;

        public AlertaController(IAlertaService alertaService, IWeatherService weatherService)
        {
            _alertaService = alertaService;
            _weatherService = weatherService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AlertaTemperatura>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AlertaTemperatura>>> GetAlertas(
            [FromQuery] string? cidade = "",
            [FromQuery] string? estado = "")
        {
            var alertas = await _alertaService.GetAlertasAsync(cidade, estado);
            return Ok(alertas);
        }

        /// <summary>
        /// Obtém um alerta específico pelo ID
        /// </summary>
        /// <response code="200">Retorna o alerta encontrado</response>
        /// <response code="404">Alerta não encontrado</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AlertaTemperatura), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AlertaTemperatura>> GetAlerta(int id)
        {
            var alerta = await _alertaService.GetAlertaByIdAsync(id);
            if (alerta == null)
                return NotFound();

            return Ok(alerta);
        }

        /// <summary>
        /// Cria um novo alerta de temperatura
        /// </summary>
        /// <response code="201">Retorna o alerta criado</response>
        /// <response code="400">Dados inválidos</response>
        [HttpPost]
        [ProducesResponseType(typeof(AlertaTemperatura), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AlertaTemperatura>> CreateAlerta([FromBody] AlertaTemperatura alerta)
        {
            var result = await _alertaService.CreateAlertaAsync(alerta);
            return CreatedAtAction(nameof(GetAlerta), new { id = result.IdAlerta }, result);
        }

        /// <summary>
        /// Atualiza o status de um alerta
        /// </summary>
        /// <response code="204">Status atualizado com sucesso</response>
        /// <response code="404">Alerta não encontrado</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var alerta = await _alertaService.GetAlertaByIdAsync(id);
            if (alerta == null)
                return NotFound();

            await _alertaService.UpdateAlertaStatusAsync(id, status);
            return NoContent();
        }

        /// <summary>
        /// Verifica as temperaturas atuais e cria alertas se necessário
        /// </summary>
        /// <response code="200">Verificação realizada com sucesso</response>
        [HttpPost("check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckTemperatures()
        {
            await _weatherService.CheckAndCreateAlertsAsync();
            return Ok();
        }
    }
}
