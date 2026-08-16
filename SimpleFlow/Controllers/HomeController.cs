using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SimpleFlow.Models;

namespace SimpleFlow.Controllers
{
    /// <summary>
    /// Контроллер главной страницы и страницы ошибок.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера главной страницы.
        /// </summary>
        /// <param name="logger">Сервис журналирования.</param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Отображает главную панель приложения.
        /// </summary>
        /// <returns>Представление главной панели.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Отображает информацию о необработанной ошибке запроса.
        /// </summary>
        /// <returns>Представление с идентификатором запроса.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
