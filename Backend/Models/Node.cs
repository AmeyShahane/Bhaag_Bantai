using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Node
    {
        public required string Label { get; set; }
        public required int XCoordinate { get; set; }
        public required int YCoordinate { get; set; }
        public List<Node> RikshawConnections { get; set; } = new();
        public List<Node> BusConnections { get; set; } = new();
        public List<Node> LocalConnections { get; set; } = new();
        public List<Node> FerryConnections { get; set; } = new();

        public Player PlayerHere { get; set; }
    }
}
