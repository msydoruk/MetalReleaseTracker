using MetalReleaseTracker.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MetalReleaseTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlbumController : ControllerBase
    {
        private readonly IAlbumService _albumService;

        public AlbumController(IAlbumService albumService)
        {
            _albumService = albumService;
        }

        [HttpGet("getTop10AlbumsFromDistributor")]
        public async Task<IActionResult> GetTop10AlbumsFromDistributor(Guid distributorId)
        {
            var albums = await _albumService.GetAlbumsByDistributor(distributorId);

            var topAlbums = albums.OrderByDescending(album => album.ReleaseDate)
                .Take(10)
                .ToList();

            return Ok(topAlbums);
        }
    }
}