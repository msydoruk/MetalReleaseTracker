﻿using MetalReleaseTracker.Core.Filters;
using MetalReleaseTracker.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MetalReleaseTracker.API.Controllers
{
    [ApiController]
    [Route("api/bands")]
    public class BandController : ControllerBase
    {
        private readonly IBandService _bandService;

        public BandController(IBandService bandService)
        {
            _bandService = bandService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFilteredBands([FromQuery] PagingAndSortingFilter baseFilter)
        {
            var bands = await _bandService.GetBandsByFilter(baseFilter);

            return Ok(bands);
        }
    }
}
