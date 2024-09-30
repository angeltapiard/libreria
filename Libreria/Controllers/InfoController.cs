using Microsoft.AspNetCore.Mvc;

namespace Libreria.Controllers
{
    public class InfoController : Controller
    {
        public IActionResult Sucursal()
        {
            return View();
        }

        public IActionResult Club()
        {
            return View();
        }
    }
}
