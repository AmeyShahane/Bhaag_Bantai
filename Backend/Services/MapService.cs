using Backend.Models;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Reflection.Emit;
using static System.Net.Mime.MediaTypeNames;

namespace Backend.Services
{
    public class MapService
    {
        private readonly Map _map;
        private readonly ILogger<MapService> _logger;
        private const int CooordinateTolerance = 10;

        public MapService(ILogger<MapService> logger) 
        {
            _map = new Map();
            _logger = logger;
            _logger.LogInformation("✅ MapService initialized\n\n");

            try
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string projectRoot = Path.Combine(baseDirectory, "..", "..", "..");
                string svgPath = Path.Combine(projectRoot, "Models", "Map.svg");

                string svgContent = File.ReadAllText(svgPath);
                LoadMap(svgContent);
                _logger.LogInformation($"✅ Map loaded with {_map.Nodes.Count} stations.\n\n");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Failed to load map: {ex.Message}\n\n");
                throw;
            }

        }
        public Map GetMap()
        {
            return _map;
        }

        private void LoadMap(string svgContent)
        {

            // Parse SVG map
            var svg = XDocument.Parse(svgContent);

            var rectangles = svg
                .Descendants()
                .Where(e => e.Name.LocalName == "g" && (string?)e.Attribute("class") == "layer")
                .SelectMany(g => g.Descendants().Where(e => e.Name.LocalName == "rect"));

            var labels = svg
                .Descendants()
                .Where(e => e.Name.LocalName == "g" && (string?)e.Attribute("class") == "layer")
                .SelectMany(g => g.Descendants().Where(e => e.Name.LocalName == "text"))
                .ToList();

            int matchedCount = 0;

            foreach (var rect in rectangles)
            {
                int cx = GetRectCenterCoordinate(rect, "x", "width");
                int cy = GetRectCenterCoordinate(rect, "y", "height");

                // Find the Nearest label
                XElement? closestText = null;
                double minDistance = double.MaxValue;
                foreach (var text in labels)
                {
                    if (!double.TryParse(text.Attribute("x")?.Value, out var tx) ||
                        !double.TryParse(text.Attribute("y")?.Value, out var ty))
                        continue;

                    double distance = Math.Sqrt(Math.Pow(tx - cx, 2) + Math.Pow(ty - cy, 2));
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestText = text;
                    }
                }
                if (closestText == null)
                {
                    var id = rect.Attribute("id")?.Value ?? "(no id)";
                    throw new Exception($"❌ Could not find a matching text label for rect ID '{id}' at ({cx},{cy})");
                }

                string label = closestText.Value.Trim();

                _map.Nodes[label] = new Node
                {
                    Label = label,
                    XCoordinate = cx,
                    YCoordinate = cy
                };
                
                matchedCount++;
            }

            // Parse SVG Paths
            var paths = svg
                .Descendants()
                .Where(e => e.Name.LocalName == "g" && (string?)e.Attribute("class") == "layer")
                .SelectMany(g => g.Descendants().Where(e => e.Name.LocalName == "path" && (string?)e.Attribute("class") == "link"));

            foreach (var path in paths)
            {
                var d = path.Attribute("d")?.Value;
                var color = path.Attribute("stroke")?.Value?.ToLower();

                if (string.IsNullOrWhiteSpace(d) || string.IsNullOrWhiteSpace(color)) continue;

                var (startX, startY) = GetPathStart(d);
                var (endX, endY) = GetPathEnd(d);

                Node? from = FindClosestNode(_map.Nodes.Values, (int)startX, (int)startY);
                Node? to = FindClosestNode(_map.Nodes.Values, (int)endX, (int)endY);

                if (from != null && to != null && from != to)
                {
                    ConnectNodes(from, to, color);
                }
            }

        }

        private void ConnectNodes(Node from, Node to, string color)
        {
            void AddBidirectional(List<Node> listFrom, List<Node> listTo)
            {
                if (!listFrom.Contains(to)) listFrom.Add(to);
                if (!listTo.Contains(from)) listTo.Add(from);
            }
            switch (color)
            {
                case "yellow":
                    AddBidirectional(from.RikshawConnections, to.RikshawConnections);
                    break;
                case "green":
                    AddBidirectional(from.BusConnections, to.BusConnections);
                    break;
                case "red":
                    AddBidirectional(from.LocalConnections, to.LocalConnections);
                    break;
                case "black":
                    AddBidirectional(from.FerryConnections, to.FerryConnections);
                    break;
                default:
                    break;
            }
        }

        private (double x, double y) GetPathEnd(string d)
        {
            var match = Regex.Match(d, @"m\s*([\d.]+),\s*([\d.]+)", RegexOptions.IgnoreCase);
            if (!match.Success) return (0, 0);

            double currentX = double.Parse(match.Groups[1].Value);
            double currentY = double.Parse(match.Groups[2].Value);

            var curveMatches = Regex.Matches(d, @"c\s*([-\d.,\s]+)", RegexOptions.IgnoreCase);
            foreach (Match curve in curveMatches)
            {
                var parts = curve.Groups[1].Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i + 5 < parts.Length; i += 6)
                {
                    currentX += double.Parse(parts[i + 4]);
                    currentY += double.Parse(parts[i + 5]);
                }
            }

            return (currentX, currentY);
        }

        private (double startX, double startY) GetPathStart(string d)
        {
            var match = Regex.Match(d, @"m\s*([\d.]+),\s*([\d.]+)", RegexOptions.IgnoreCase);
            return match.Success
                ? (double.Parse(match.Groups[1].Value), double.Parse(match.Groups[2].Value))
                : (0, 0);
        }

        private Node? FindClosestNode(IEnumerable<Node> nodes, int x, int y)
        {
            return nodes
                .OrderBy(n => Math.Sqrt(Math.Pow(n.XCoordinate - x, 2) + Math.Pow(n.YCoordinate - y, 2)))
                .FirstOrDefault(n =>
                    Math.Abs(n.XCoordinate - x) <= CooordinateTolerance &&
                    Math.Abs(n.YCoordinate - y) <= CooordinateTolerance
                    );
        }

        private int GetRectCenterCoordinate(XElement el, string positionAttribute, string sizeAttribute)
        {
            var position = el.Attribute(positionAttribute)?.Value;
            var size = el.Attribute(sizeAttribute)?.Value;
            if (position == null || size == null)
            {
                var id = el.Attribute("id")?.Value ?? "(no id)";
                throw new Exception($"❌ Missing '{position}' or '{size}' in <rect> element ID: {id}");
            }
            int intpos = (int)double.Parse(position);
            int intsize = (int)double.Parse(size);

            return intpos + intsize / 2;
        }
    }
}
