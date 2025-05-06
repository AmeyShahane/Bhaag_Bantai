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
        private readonly ILogger<MapController> _logger;

        public MapController(MapService mapService, ILogger<MapController> logger)
        {
            _mapService = mapService;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<Map> GetMap()
        {
            _logger.LogInformation("Starting to fetch the map...\n\n");
            var map = _mapService.GetMap();
            if (map.Nodes.Count == 0)
            {
                _logger.LogWarning("No nodes found in the map.\n\n");
            }
            return Ok(map);
        }
    }
}