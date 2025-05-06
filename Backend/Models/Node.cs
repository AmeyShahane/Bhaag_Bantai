using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Node
    {
        public required string Label { get; set; }
        public required int XCoordinate { get; set; }
        public required int YCoordinate { get; set; }
        public List<string> RikshawConnections { get; set; } = new();
        public List<string> BusConnections { get; set; } = new();
        public List<string> LocalConnections { get; set; } = new();
        public List<string> FerryConnections { get; set; } = new();

        public Player PlayerHere { get; set; }
    }
}
