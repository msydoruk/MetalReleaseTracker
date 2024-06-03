using Microsoft.AspNetCore.Mvc;

namespace MetalReleaseTracker.API.Controllers
{
    public class BandController : Controller
    {
        public IActionResult Index()
        {
            return this.View();
        }
    }
}
