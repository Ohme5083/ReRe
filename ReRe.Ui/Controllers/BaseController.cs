using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ReRe.Ui.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!HttpContext.Session.Keys.Contains("Id"))
            {
                HttpContext.Session.SetInt32("Id", 0);
                HttpContext.Session.SetString("UserEmail", "");
                HttpContext.Session.SetString("UserPassword", "");
                HttpContext.Session.SetString("UserName", "");
                HttpContext.Session.SetString("UserFirstName", "");
                HttpContext.Session.SetInt32("RoleId", 0);
            }
            ViewBag.Id = HttpContext.Session.GetInt32("Id");
            ViewBag.RoleId = HttpContext.Session.GetInt32("RoleId");
            ViewBag.Email = HttpContext.Session.GetString("UserEmail");
            ViewBag.Password = HttpContext.Session.GetString("UserPassword");
            ViewBag.Nom = HttpContext.Session.GetString("UserName");
            ViewBag.Prenom = HttpContext.Session.GetString("UserFirstName");
            base.OnActionExecuting(context);
        }
    }
}
