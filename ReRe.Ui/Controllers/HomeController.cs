using Microsoft.AspNetCore.Mvc;
using ReRe.Data.DbContext;
using ReRe.Ui.Models;
using System.Linq;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ReRe.Ui.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ReReDbContext _context;

        public HomeController(ILogger<HomeController> logger, ReReDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            // Récupérer les 5 dernières ressources triées par date de création décroissante
            ViewBag.latestResources = await _context.Ressources
                .OrderByDescending(r => r.CreationDate)
                .Take(5)
                .ToListAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
