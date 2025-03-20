using Microsoft.AspNetCore.Mvc;

namespace NerdStore.WebAPI.Controllers
{
    public class CarrinhoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
