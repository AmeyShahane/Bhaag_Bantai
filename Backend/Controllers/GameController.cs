using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameService _gameService;

        public GameController(GameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost("add-pandu")]
        public IActionResult AddPandu([FromBody] string playername)
        {
            bool success = _gameService.AddPandu(playername);
            if (!success)
                return BadRequest("Maximum of 5 Pandus already added.");

            return Ok("Pandu added successfully.");
        }

        [HttpPost("add-bantai")]
        public IActionResult AddBantai([FromBody] string playername)
        {
            bool success = _gameService.AddBantai(playername);
            if (!success)
                return BadRequest("There is already a Bantai.");

            return Ok("Bantai added successfully.");
        }

        [HttpGet("state")]
        public ActionResult<GameState> GetState()
        {
            return Ok(_gameService.GetState());
        }
        [HttpGet("map")]
        public ActionResult<GameState> GetMap()
        {
            return Ok(_gameService.GetMap());
        }
    }
}
