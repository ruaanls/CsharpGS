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
    [SwaggerTag(@"Sistema de Preferências de Notificação: Gerenciamento de preferências para alertas de temperatura.
    Recursos disponíveis:
    - Cadastro de novas preferências por cidade/estado
    - Consulta de preferências existentes com filtros
    - Atualização de limites de temperatura
    - Ativação/desativação de notificações
    - Exclusão de preferências
    Cada preferência permite definir:
    - Cidade e estado para monitoramento
    - Limites mínimo e máximo de temperatura
    - Status de ativação das notificações")]
    public class PreferenciasController : ControllerBase
    {
        private readonly IPreferenciasService _preferenciasService;

        public PreferenciasController(IPreferenciasService preferenciasService)
        {
            _preferenciasService = preferenciasService;
        }        /// <summary>
        /// Lista todas as preferências de notificação
        /// </summary>
        /// <remarks>
        /// Retorna a lista de todas as preferências cadastradas, com opção de filtrar por cidade e estado.
        /// 
        /// Exemplo de requisição:
        /// ```http
        /// GET /api/Preferencias?cidade=São Paulo&amp;estado=SP
        /// ```
        /// 
        /// O resultado inclui:
        /// * ID da preferência
        /// * Cidade e estado monitorados
        /// * Limites de temperatura configurados
        /// * Status de ativação
        /// * Datas de criação e última atualização
        /// </remarks>
        /// <param name="cidade">Filtra preferências por cidade específica</param>
        /// <param name="estado">Filtra preferências por estado específico</param>
        /// <response code="200">Lista de preferências encontradas</response>
        /// <response code="204">Nenhuma preferência encontrada com os filtros fornecidos</response>
        /// <response code="400">Dados inválidos ou regras de validação não atendidas</response>
        /// <response code="500">Erro ao processar a requisição</response>
        [HttpGet]
        [ProducesResponseType(typeof(HypermediaResponse<IEnumerable<PreferenciasNotificacao>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HypermediaResponse<IEnumerable<PreferenciasNotificacao>>>> GetPreferencias(
            [FromQuery] string? cidade = "",
            [FromQuery] string? estado = "")
        {
            var preferencias = await _preferenciasService.GetPreferenciasAsync(cidade, estado);
            
            var response = new HypermediaResponse<IEnumerable<PreferenciasNotificacao>>
            {
                Data = preferencias,
                Links = new Dictionary<string, Link>
                {
                    { "self", new Link($"/api/Preferencias?cidade={cidade}&estado={estado}") },
                    { "create", new Link("/api/Preferencias", "POST") },
                    { "alerts", new Link($"/api/Alerta?cidade={cidade}&estado={estado}") }
                }
            };

            foreach (var pref in preferencias)
            {
                response.Links[$"preferencia_{pref.IdPreferencia}"] = new Link($"/api/Preferencias/{pref.IdPreferencia}");
            }

            return Ok(response);
        }

        /// <summary>
        /// Obtém uma preferência específica pelo ID
        /// </summary>
        /// <param name="id">ID da preferência</param>
        /// <returns>Detalhes da preferência</returns>
        /// <response code="200">Retorna a preferência encontrada</response>
        /// <response code="404">Preferência não encontrada</response>
        /// <response code="500">Erro ao processar a requisição</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(HypermediaResponse<PreferenciasNotificacao>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HypermediaResponse<PreferenciasNotificacao>>> GetPreferencia(int id)
        {
            var preferencia = await _preferenciasService.GetPreferenciaByIdAsync(id);
            if (preferencia == null)
            {
                var error = new ErrorResponse("PREFERENCE_NOT_FOUND", "Preferência não encontrada");
                error.AddLink("documentation", "/docs/errors/PREFERENCE_NOT_FOUND");
                error.AddLink("all_preferences", "/api/Preferencias");
                return NotFound(error);
            }

            var response = new HypermediaResponse<PreferenciasNotificacao>
            {
                Data = preferencia,
                Links = new Dictionary<string, Link>
                {
                    { "self", new Link($"/api/Preferencias/{id}") },
                    { "all", new Link("/api/Preferencias") },
                    { "update", new Link($"/api/Preferencias/{id}", "PUT") },
                    { "delete", new Link($"/api/Preferencias/{id}", "DELETE") },
                    { "alerts", new Link($"/api/Alerta?cidade={preferencia.Cidade}&estado={preferencia.Estado}") }
                }
            };

            return Ok(response);
        }

        /// <summary>
        /// Cria uma nova preferência de notificação
        /// </summary>        /// <summary>
        /// Cria uma nova preferência de notificação
        /// </summary>
        /// <remarks>
        /// Cria uma nova configuração de preferência para monitoramento de temperatura.
        /// 
        /// Exemplo de requisição:
        /// ```json
        /// {
        ///     "cidade": "São Paulo",
        ///     "estado": "SP",
        ///     "temperaturaMin": 15,
        ///     "temperaturaMax": 30,
        ///     "ativo": true
        /// }
        /// ```
        /// 
        /// Regras de validação:
        /// * Cidade e estado são obrigatórios
        /// * Pelo menos um limite de temperatura (mínimo ou máximo) deve ser definido
        /// * Temperatura mínima deve ser menor que a máxima
        /// * Estado deve ser uma UF válida do Brasil
        /// </remarks>
        /// <param name="preferencia">Dados da nova preferência</param>
        /// <response code="201">Preferência criada com sucesso</response>
        /// <response code="400">Dados inválidos ou regras de validação não atendidas</response>
        /// <response code="500">Erro ao processar a requisição</response>
        [HttpPost]
        [ProducesResponseType(typeof(HypermediaResponse<PreferenciasNotificacao>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HypermediaResponse<PreferenciasNotificacao>>> CreatePreferencia([FromBody] PreferenciasNotificacao preferencia)
        {
            try 
            {
                var result = await _preferenciasService.CreatePreferenciaAsync(preferencia);
                
                var response = new HypermediaResponse<PreferenciasNotificacao>
                {
                    Data = result,
                    Links = new Dictionary<string, Link>
                    {
                        { "self", new Link($"/api/Preferencias/{result.IdPreferencia}") },
                        { "update", new Link($"/api/Preferencias/{result.IdPreferencia}", "PUT") },
                        { "delete", new Link($"/api/Preferencias/{result.IdPreferencia}", "DELETE") },
                        { "all", new Link("/api/Preferencias") },
                        { "alerts", new Link($"/api/Alerta?cidade={result.Cidade}&estado={result.Estado}") }
                    }
                };

                return CreatedAtAction(nameof(GetPreferencia), new { id = result.IdPreferencia }, response);
            }
            catch (Exception ex)
            {
                var error = new ErrorResponse("CREATION_ERROR", ex.Message);
                error.AddLink("documentation", "/docs/errors/CREATION_ERROR");
                error.AddLink("support", "https://weatheralert.com/support");
                return BadRequest(error);
            }
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
        /// <response code="500">Erro ao processar a requisição</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
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
        /// <response code="500">Erro ao processar a requisição</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePreferencia(int id)
        {
            await _preferenciasService.DeletePreferenciaAsync(id);
            return NoContent();
        }
    }
}
