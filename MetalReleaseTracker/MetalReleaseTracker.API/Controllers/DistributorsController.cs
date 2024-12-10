using MetalReleaseTracker.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MetalReleaseTracker.API.Controllers
{
    [ApiController]
    [Route("api/distributors")]
    public class DistributorsController : ControllerBase
    {
        private readonly IDistributorsService _distributorsService;

        public DistributorsController(IDistributorsService distributorsService)
        {
            _distributorsService = distributorsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDistributors()
        {
            var bands = await _distributorsService.GetAllDistributors();

            return Ok(bands);
        }
    }
}
