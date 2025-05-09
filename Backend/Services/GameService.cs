using Backend.DTO;
using Backend.Models;

namespace Backend.Services
{
    public class GameService
    {
        public GameState GameState { get; private set; }
        private readonly ILogger<GameService> _logger;

        public GameService(MapService mapService,  ILogger<GameService> logger)
        {
            _logger = logger;
            _logger.LogInformation("GameService initialized. Loading game...\n\n");
            GameState = new GameState
            {
                Map = mapService.GetMap()
            };
            _logger.LogInformation("GameState and Map loaded successfully.\n\n");
        }

        public bool AddPandu(string playername)
        {
            if (GameState.Pandu.Count >= 5)
            {
                _logger.LogWarning("Attempted to add Pandu but limit reached. PlayerId: {id}", playername);
                return false;
            }
            GameState.AddPandu(playername);
            _logger.LogInformation("Added Pandu: {name}", playername);
            return true;
        }

        public bool AddBantai(string playername)
        {
            if (GameState.Bantai != null)
            {
                _logger.LogWarning("Attempted to add bantai but limit reached. PlayerId: {id}", playername);
                return false;
            }
            GameState.AddBantai(playername);
            _logger.LogInformation("Added Bantai: {name}", playername);
            return true;
        }

        public GameStateDto GetState() 
        {
            return new GameStateDto
            {
                Pandu = GameState.Pandu.Select(p => new PlayerDto
                {
                    PlayerName = p.PlayerName,
                    Color = p.Color,
                    IsBantai = false,
                    StartingPosition = p.StartingPosition.Label,
                    CurrentPosition = p.CurrentPosition.Label
                }).ToList(),
                Bantai = GameState.Bantai == null ? null : new PlayerDto
                {
                    PlayerName = GameState.Bantai.PlayerName,
                    Color = GameState.Bantai.Color,
                    IsBantai = true,
                    StartingPosition = GameState.Bantai.StartingPosition.Label,
                    CurrentPosition = GameState.Bantai.CurrentPosition.Label
                },
            };

        }
        public MapDTO GetMap()
        {
            return new MapDTO
            {
                Nodes = GameState.Map.Nodes.ToDictionary(
                kvp => kvp.Key,
                kvp =>
                    {
                        var node = kvp.Value;
                        return new NodeDto
                        {
                            Label = node.Label,
                            XCoordinate = node.XCoordinate,
                            YCoordinate = node.YCoordinate,
                            RikshawConnections = node.RikshawConnections.Select(n => n.Label).ToList(),
                            BusConnections = node.BusConnections.Select(n => n.Label).ToList(),
                            LocalConnections = node.LocalConnections.Select(n => n.Label).ToList(),
                            FerryConnections = node.FerryConnections.Select(n => n.Label).ToList(),
                            PlayerNameHere = node.PlayerHere?.PlayerName
                        };
                    }
                )

            };
        }
    }
}
