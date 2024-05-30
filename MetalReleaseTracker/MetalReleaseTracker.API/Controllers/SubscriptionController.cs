using Microsoft.AspNetCore.Mvc;

namespace MetalReleaseTracker.API.Controllers
{
    public class SubscriptionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
