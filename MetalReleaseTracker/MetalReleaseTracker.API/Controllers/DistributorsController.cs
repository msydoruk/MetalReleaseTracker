using Microsoft.AspNetCore.Mvc;

namespace MetalReleaseTracker.API.Controllers
{
    public class DistributorsController : Controller
    {
        public IActionResult Index()
        {
            return this.View();
        }
    }
}
