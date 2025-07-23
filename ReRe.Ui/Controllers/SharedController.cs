using Microsoft.AspNetCore.Mvc;

namespace ReRe.Ui.Controllers
{
    public class SharedController : Controller
    {
        public IActionResult _Layout()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var password = HttpContext.Session.GetString("UserPassword");
            var Nom = HttpContext.Session.GetString("UserName");
            var Prenom = HttpContext.Session.GetString("UserFirstName");
            var RoleId = HttpContext.Session.GetInt32("RoleId");
            var id = HttpContext.Session.GetInt32("Id");

            ViewBag.Email = email;
            ViewBag.Password = password;
            ViewBag.Nom = Nom;
            ViewBag.Prenom = Prenom;
            ViewBag.Id = id;
            ViewBag.RoleId = RoleId;

            return View();
        }
    }
}
