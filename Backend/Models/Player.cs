namespace Backend.Models
{
    public class Player
    {
        public required string PlayerName { get; set; }
        public required string Color { get; set; }
        public required Node StartingPosition { get; set; }
        public required Node CurrentPosition { get; set; }
        public required bool IsBantai { get; set; }

    }
}
