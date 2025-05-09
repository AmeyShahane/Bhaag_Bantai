namespace Backend.DTO
{
    public class NodeDto
    {
        public string Label { get; set; }
        public int XCoordinate { get; set; }
        public int YCoordinate { get; set; }

        public List<string> RikshawConnections { get; set; }
        public List<string> BusConnections { get; set; }
        public List<string> LocalConnections { get; set; }
        public List<string> FerryConnections { get; set; }

        public string? PlayerNameHere { get; set; }
    }
}
