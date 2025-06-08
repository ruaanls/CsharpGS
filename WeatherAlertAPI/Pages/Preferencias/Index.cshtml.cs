using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WeatherAlertAPI.Models;
using WeatherAlertAPI.Services;

namespace WeatherAlertAPI.Pages.Preferencias
{
    public class IndexModel : PageModel
    {
        private readonly IPreferenciasService _preferenciasService;

        public IndexModel(IPreferenciasService preferenciasService)
        {
            _preferenciasService = preferenciasService;
        }

        public IList<PreferenciasNotificacao> Preferencias { get; set; } = new List<PreferenciasNotificacao>();
        public PreferenciaFiltro Filtro { get; set; } = new PreferenciaFiltro();

        public async Task OnGetAsync()
        {
            Preferencias = await _preferenciasService.ObterPreferenciasAsync(Filtro);
        }
    }
} 