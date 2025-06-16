using System.Text;
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

        public string GetMapSvg()
        {
            var svgHeader = @"<svg xmlns='http://www.w3.org/2000/svg' width='75%' height='100%' viewBox='0 -100 1610 1350'>
                <style>
                    .node { stroke: #333; stroke-width: 1; }
                    .label { font-size: 12px; fill: black;font-weight: bold; text-anchor: middle; }
                    .yellow { fill: #fafa05; }
                    .green { fill: green; }
                    .red { fill: red; }
                </style>
                <rect width='100%'; height='100%' fill='#a6bf84' />
            ";

            var svgBody = new StringBuilder();

            var renderedFerryConnections = new HashSet<string>();
            foreach (var node in GameState.Map.Nodes.Values)
            {
                foreach (var targetId in node.FerryConnections)
                {
                    var targetNode = GameState.Map.Nodes[targetId.Label];

                    var key = string.Compare(node.Label, targetNode.Label) < 0
                    ? $"{node.Label}-{targetNode.Label}"
                    : $"{targetNode.Label}-{node.Label}";

                    if (renderedFerryConnections.Contains(key))
                        continue;

                    renderedFerryConnections.Add(key);

                    // Control points logic
                    var startX = node.XCoordinate;
                    var startY = node.YCoordinate;
                    var endX = targetNode.XCoordinate;
                    var endY = targetNode.YCoordinate;

                    var dx = endX - startX;
                    var dy = endY - startY;
                    var ctrl1X = startX + dx / 3;
                    var ctrl1Y = startY - 40;
                    var ctrl2X = endX - dx / 3;
                    var ctrl2Y = endY - 40;

                    svgBody.AppendFormat(@"
            <path d='M {0} {1} C {2} {3}, {4} {5}, {6} {7}'
                  stroke='black' fill='transparent' stroke-width='2' class='link' />",
                        startX, startY, ctrl1X, ctrl1Y, ctrl2X, ctrl2Y, endX, endY);
                }
            }

            var renderedLocalConnections = new HashSet<string>();
            foreach (var node in GameState.Map.Nodes.Values)
            {
                foreach (var targetId in node.LocalConnections)
                {
                    var targetNode = GameState.Map.Nodes[targetId.Label];
                    var key = string.Compare(node.Label, targetNode.Label) < 0
                    ? $"{node.Label}-{targetNode.Label}"
                    : $"{targetNode.Label}-{node.Label}";

                    if (renderedLocalConnections.Contains(key))
                        continue;

                    renderedLocalConnections.Add(key);
                    // Control points logic
                    var startX = node.XCoordinate;
                    var startY = node.YCoordinate;
                    var endX = targetNode.XCoordinate;
                    var endY = targetNode.YCoordinate;

                    var dx = endX - startX;
                    var dy = endY - startY;
                    var ctrl1X = startX + dx / 3;
                    var ctrl1Y = startY - 40;
                    var ctrl2X = endX - dx / 3;
                    var ctrl2Y = endY - 40;

                    svgBody.AppendFormat(@"
<path d='M {0} {1} C {2} {3}, {4} {5}, {6} {7}'
      stroke='red' fill='transparent' stroke-width='2' stroke-dasharray='4,2' class='link' />",
    startX, startY, ctrl1X, ctrl1Y, ctrl2X, ctrl2Y, endX, endY);
                }
            }

            var renderedBusConnections = new HashSet<string>();
            foreach (var node in GameState.Map.Nodes.Values)
            {
                foreach (var targetId in node.BusConnections)
                {
                    var targetNode = GameState.Map.Nodes[targetId.Label];
                    var key = string.Compare(node.Label, targetNode.Label) < 0
                    ? $"{node.Label}-{targetNode.Label}"
                    : $"{targetNode.Label}-{node.Label}";

                    if (renderedBusConnections.Contains(key))
                        continue;

                    renderedBusConnections.Add(key);

                    // Control points logic
                    var startX = node.XCoordinate;
                    var startY = node.YCoordinate;
                    var endX = targetNode.XCoordinate;
                    var endY = targetNode.YCoordinate;


                    var offsetY = -10;
                    var offsetX = -15;
                    var dx = endX - startX;
                    var dy = endY - startY;
                    var ctrl1X = offsetX + startX + dx / 3;
                    var ctrl1Y = startY + dy / 3 + offsetY;
                    var ctrl2X = offsetX + endX - dx / 3;
                    var ctrl2Y = endY - dy / 3 + offsetY;

                    svgBody.AppendFormat(@"
            <path d='M {0} {1} C {2} {3}, {4} {5}, {6} {7}'
                  stroke='green' fill='transparent' stroke-width='2' class='link' />",
                        startX, startY, ctrl1X, ctrl1Y, ctrl2X, ctrl2Y, endX, endY);
                }
            }

            var renderedRikshawConnections = new HashSet<string>();
            foreach (var node in GameState.Map.Nodes.Values)
            {
                foreach (var targetId in node.RikshawConnections)
                {
                    var targetNode = GameState.Map.Nodes[targetId.Label];

                    var key = string.Compare(node.Label, targetNode.Label) < 0
                    ? $"{node.Label}-{targetNode.Label}"
                    : $"{targetNode.Label}-{node.Label}";

                    if (renderedRikshawConnections.Contains(key))
                        continue;

                    renderedRikshawConnections.Add(key);

                    // Control points logic
                    var startX = node.XCoordinate;
                    var startY = node.YCoordinate;
                    var endX = targetNode.XCoordinate;
                    var endY = targetNode.YCoordinate;

                    var dx = endX - startX;
                    var dy = endY - startY;
                    var ctrl1X = startX + dx / 3;
                    var ctrl1Y = startY + dy / 3;
                    var ctrl2X = endX - dx / 3;
                    var ctrl2Y = endY - dy / 3;

                    svgBody.AppendFormat(@"
            <path d='M {0} {1} C {2} {3}, {4} {5}, {6} {7}'
                  stroke='yellow' fill='transparent' stroke-width='2' class='link' />",
                        startX, startY, ctrl1X, ctrl1Y, ctrl2X, ctrl2Y, endX, endY);
                }
            }


            foreach (var node in GameState.Map.Nodes.Values)
            {
                
                // Add small circles if there are connections
                if (node.RikshawConnections.Any() && !node.BusConnections.Any())
                {
                    // Only RikshawConnection, add small yellow circles
                    svgBody.AppendFormat(@"<circle cx='{0}' cy='{1}' r='8' class='yellow' />", node.XCoordinate, node.YCoordinate - 2); // Top circle
                    svgBody.AppendFormat(@"<circle cx='{0}' cy='{1}' r='8' class='yellow' />", node.XCoordinate, node.YCoordinate + 2); // Bottom circle
                }
                else if (node.RikshawConnections.Any() && node.BusConnections.Any())
                {
                    // Both connections, add green on top and yellow on bottom
                    svgBody.AppendFormat(@"<circle cx='{0}' cy='{1}' r='8' class='green' />", node.XCoordinate, node.YCoordinate - 2); // Top circle (green)
                    svgBody.AppendFormat(@"<circle cx='{0}' cy='{1}' r='8' class='yellow' />", node.XCoordinate, node.YCoordinate + 2); // Bottom circle (yellow)
                }
                // Determine the color of the node (rect) based on LocalConnection
                string nodeColor = (node.LocalConnections != null && node.LocalConnections.Any()) ? "red" : "yellow";

                svgBody.AppendFormat(@"<rect x='{0}' y='{1}' width='20' height='10' class='node {2}' />
               <text x='{3}' y='{4}' dy='.35em' class='label'>{5}</text>",
                node.XCoordinate - 10, node.YCoordinate - 5, nodeColor,
                node.XCoordinate, node.YCoordinate, node.Label); // Center the square and the label
            }

            var svgFooter = "</svg>";

            // Combine header, body, and footer to generate the final SVG content
            return svgHeader + svgBody.ToString() + svgFooter;
        }


    }
}
