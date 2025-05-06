using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ControllerBase
    {
        private readonly MapService _mapService;

        public MapController(MapService mapService)
        {
            _mapService = mapService;
        }

        [HttpGet]
        public ActionResult<Map> GetMap()
        {
            var map = _mapService.GetMap();
            return Ok(map);
        }
    }
}