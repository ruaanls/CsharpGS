using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeatherAlertAPI.Models;
using WeatherAlertAPI.Services;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Http;

namespace WeatherAlertAPI.Controllers
{    /// <summary>
    /// Sistema de Alertas de Temperatura
    /// </summary>
    /// <remarks>
    /// API para gerenciamento e monitoramento de alertas de temperatura.
    /// 
    /// Funcionalidades principais:
    /// * Monitoramento em tempo real de temperaturas
    /// * Geração automática de alertas baseados em preferências
    /// * Consulta de histórico de alertas
    /// * Atualização de status de alertas
    /// * Integração com serviço meteorológico externo
    /// 
    /// Tipos de Alertas:
    /// * ALTO: Temperatura acima do limite máximo
    /// * BAIXO: Temperatura abaixo do limite mínimo
    /// * NORMAL: Temperatura dentro dos limites
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "v1")]
    [SwaggerTag("Sistema de Alertas")]
    public class AlertaController : ControllerBase
    {
        private readonly IAlertaService _alertaService;
        private readonly IWeatherService _weatherService;

        public AlertaController(IAlertaService alertaService, IWeatherService weatherService)
        {
            _alertaService = alertaService;
            _weatherService = weatherService;
        }        /// <summary>
        /// Lista todos os alertas de temperatura
        /// </summary>
        /// <remarks>
        /// Retorna todos os alertas de temperatura gerados, com opção de filtrar por cidade e estado.
        /// 
        /// Exemplo de requisição:
        /// ```http
        /// GET /api/Alerta?cidade=São Paulo&amp;estado=SP
        /// ```
        /// 
        /// **Estrutura da Resposta (HypermediaResponse):**
        /// A resposta é um objeto `HypermediaResponse` contendo a lista de alertas e links de navegação HATEOAS.
        /// - **`data`**: Uma lista de objetos `AlertaTemperatura`, cada um com os detalhes do alerta.
        /// - **`_links`**: Um objeto contendo URIs relacionadas para navegação. Inclui:
        ///   - **`self`**: Link para a requisição atual.
        ///   - **`check`**: Link para verificar e criar novos alertas (método POST).
        ///   - **`preferences`**: Link para as preferências de notificação relacionadas à cidade/estado.
        ///   - **`alerta_{id}`**: Links individuais para cada alerta retornado na lista, permitindo acesso direto aos detalhes de um alerta específico.
        /// </remarks>
        /// <param name="cidade">Filtra alertas por cidade específica</param>
        /// <param name="estado">Filtra alertas por estado específico</param>
        /// <response code="200">
        /// Retorna um `HypermediaResponse` contendo uma lista de `AlertaTemperatura` e links de navegação.
        /// 
        /// Exemplo de Resposta:
        /// ```json
        /// {
        ///   "data": [
        ///     {
        ///       "idAlerta": 1,
        ///       "cidade": "São Paulo",
        ///       "estado": "SP",
        ///       "temperatura": 32.5,
        ///       "tipoAlerta": "ALTO",
        ///       "mensagem": "Temperatura acima do limite máximo.",
        ///       "dataHora": "2025-06-08T10:00:00Z",
        ///       "status": "ATIVO"
        ///     },
        ///     {
        ///       "idAlerta": 2,
        ///       "cidade": "Rio de Janeiro",
        ///       "estado": "RJ",
        ///       "temperatura": 12.1,
        ///       "tipoAlerta": "BAIXO",
        ///       "mensagem": "Temperatura abaixo do limite mínimo.",
        ///       "dataHora": "2025-06-08T11:30:00Z",
        ///       "status": "ATIVO"
        ///     }
        ///   ],
        ///   "_links": {
        ///     "self": {
        ///       "href": "/api/Alerta?cidade=São%20Paulo&estado=SP",
        ///       "method": "GET"
        ///     },
        ///     "check": {
        ///       "href": "/api/Alerta/check",
        ///       "method": "POST"
        ///     },
        ///     "preferences": {
        ///       "href": "/api/Preferencias?cidade=São%20Paulo&estado=SP",
        ///       "method": "GET"
        ///     },
        ///     "alerta_1": {
        ///       "href": "/api/Alerta/1",
        ///       "method": "GET"
        ///     },
        ///     "alerta_2": {
        ///       "href": "/api/Alerta/2",
        ///       "method": "GET"
        ///     }
        ///   }
        /// }
        /// ```
        /// </response>
        /// <response code="204">Nenhum alerta encontrado com os filtros fornecidos</response>
        /// <response code="400">Dados inválidos (ex: parâmetros de query mal formatados)</response>
        /// <response code="500">Erro interno do servidor ao processar a requisição ou acessar o serviço de previsão do tempo.</response>
        [HttpGet]
        [ProducesResponseType(typeof(HypermediaResponse<IEnumerable<AlertaTemperatura>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HypermediaResponse<IEnumerable<AlertaTemperatura>>>> GetAlertas(
            [FromQuery] string? cidade = "",
            [FromQuery] string? estado = "")
        {
            var alertas = await _alertaService.GetAlertasAsync(cidade, estado);
            
            var response = new HypermediaResponse<IEnumerable<AlertaTemperatura>>
            {
                Data = alertas,
                Links = new Dictionary<string, Link>
                {
                    { "self", new Link($"/api/Alerta?cidade={cidade}&estado={estado}") },
                    { "check", new Link("/api/Alerta/check", "POST") },
                    { "preferences", new Link($"/api/Preferencias?cidade={cidade}&estado={estado}") }
                }
            };

            foreach (var alerta in alertas)
            {
                response.Links[$"alerta_{alerta.IdAlerta}"] = new Link($"/api/Alerta/{alerta.IdAlerta}");
            }

            return Ok(response);
        }

        /// <summary>
        /// Obtém um alerta específico pelo ID
        /// </summary>
        /// <response code="200">Retorna o alerta encontrado</response>
        /// <response code="404">Alerta não encontrado</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(HypermediaResponse<AlertaTemperatura>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HypermediaResponse<AlertaTemperatura>>> GetAlerta(int id)
        {
            var alerta = await _alertaService.GetAlertaByIdAsync(id);
            if (alerta == null)
            {
                var error = new ErrorResponse("ALERT_NOT_FOUND", "Alerta não encontrado");
                error.AddLink("documentation", "/docs/errors/ALERT_NOT_FOUND");
                error.AddLink("all_alerts", "/api/Alerta");
                return NotFound(error);
            }

            var response = new HypermediaResponse<AlertaTemperatura>
            {
                Data = alerta,
                Links = new Dictionary<string, Link>
                {
                    { "self", new Link($"/api/Alerta/{id}") },
                    { "all", new Link("/api/Alerta") },
                    { "update_status", new Link($"/api/Alerta/{id}/status", "PUT") },
                    { "preferences", new Link($"/api/Preferencias?cidade={alerta.Cidade}&estado={alerta.Estado}") }
                }
            };

            return Ok(response);
        }

        /// <summary>
        /// Cria um novo alerta de temperatura
        /// </summary>
        /// <response code="201">Retorna o alerta criado</response>
        /// <response code="400">Dados inválidos</response>
        [HttpPost]
        [ProducesResponseType(typeof(HypermediaResponse<AlertaTemperatura>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<HypermediaResponse<AlertaTemperatura>>> CreateAlerta([FromBody] AlertaTemperatura alerta)
        {
            try
            {
                var result = await _alertaService.CreateAlertaAsync(alerta);
                
                var response = new HypermediaResponse<AlertaTemperatura>
                {
                    Data = result,
                    Links = new Dictionary<string, Link>
                    {
                        { "self", new Link($"/api/Alerta/{result.IdAlerta}") },
                        { "all", new Link("/api/Alerta") },
                        { "update_status", new Link($"/api/Alerta/{result.IdAlerta}/status", "PUT") },
                        { "preferences", new Link($"/api/Preferencias?cidade={result.Cidade}&estado={result.Estado}") }
                    }
                };

                return CreatedAtAction(nameof(GetAlerta), new { id = result.IdAlerta }, response);
            }
            catch (Exception ex)
            {
                var error = new ErrorResponse("CREATE_ERROR", ex.Message);
                error.AddLink("documentation", "/docs/errors/CREATE_ERROR");
                error.AddLink("support", "https://weatheralert.com/support");
                return BadRequest(error);
            }
        }

        /// <summary>
        /// Atualiza o status de um alerta
        /// </summary>
        /// <response code="204">Status atualizado com sucesso</response>
        /// <response code="404">Alerta não encontrado</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var alerta = await _alertaService.GetAlertaByIdAsync(id);
            if (alerta == null)
                return NotFound();

            await _alertaService.UpdateAlertaStatusAsync(id, status);
            return NoContent();
        }        /// <summary>
        /// Verifica as temperaturas atuais e cria alertas se necessário
        /// </summary>
        /// <remarks>
        /// Este endpoint realiza as seguintes ações:
        /// 
        /// 1. Consulta a temperatura atual de todas as cidades monitoradas
        /// 2. Compara com as preferências de notificação existentes
        /// 3. Cria alertas automaticamente quando necessário
        /// 
        /// Exemplo de requisição:
        /// ```http
        /// POST /api/Alerta/check
        /// ```
        /// 
        /// Possíveis resultados:
        /// - Alertas de temperatura alta
        /// - Alertas de temperatura baixa
        /// - Nenhum alerta (temperaturas normais)
        /// </remarks>
        /// <response code="200">Verificação realizada com sucesso</response>
        /// <response code="500">Erro ao acessar o serviço de previsão do tempo</response>
        [HttpPost("check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckTemperatures()
        {
            await _weatherService.CheckAndCreateAlertsAsync();
            return Ok();
        }
    }
}
