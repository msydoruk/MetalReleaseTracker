using Microsoft.AspNetCore.Mvc;

namespace MetalReleaseTracker.API.Controllers
{
    public class AlbumController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
