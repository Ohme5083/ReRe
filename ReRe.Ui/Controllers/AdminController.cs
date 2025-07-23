using Microsoft.AspNetCore.Mvc;

namespace ReRe.Ui.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
