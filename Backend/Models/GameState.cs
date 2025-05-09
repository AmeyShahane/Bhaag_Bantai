using System.Collections.Generic;

namespace Backend.Models
{
    public class GameState
    {
        public List<Player> Pandu { get; set; } = new();
        public Player Bantai { get; set; }
        public int TotalPlayers => Pandu.Count + (Bantai != null ? 1 : 0);
        public Map Map { get; set; }
        public bool GameInitiated { get; set; } = false;
        string[] colors = { "Red", "Blue", "Green", "Yellow", "Purple" };
        int[] AllStartingPositions = [13,26,29,34,42,52,53,75,78,91,94,103,112,117,132,138,141,157];
        private readonly Random random = new();


        public bool AddPandu(string name)
        {
            if (Pandu.Count >= colors.Length) return false; 

            int position = GetRandomAvailablePosition();
            if (position == -1) return false;

            var player = new Player
            {
                PlayerName = name,
                Color = colors[Pandu.Count],
                IsBantai = false,
                StartingPosition = Map.Nodes[position.ToString()],
                CurrentPosition = Map.Nodes[position.ToString()]
            };

            Pandu.Add(player);
            return true;
        }

        public bool AddBantai(string name)
        {
            if (Bantai != null) return false;

            int position = GetRandomAvailablePosition();
            if (position == -1) return false;

            var player = new Player
            {
                PlayerName = name,
                Color = "Black",
                IsBantai = true,
                StartingPosition = Map.Nodes[position.ToString()],
                CurrentPosition = Map.Nodes[position.ToString()]
            };

            return true;
        }
        private int GetRandomAvailablePosition()
        {
            var usedPositions = new HashSet<string>(
                Pandu.Select(p => p.CurrentPosition.Label)
            );

            if (Bantai != null)
                usedPositions.Add(Bantai.CurrentPosition.Label);

            var available = AllStartingPositions
                .Select(p => p.ToString())
                .Where(pos => !usedPositions.Contains(pos))
                .ToList();

            if (!available.Any()) return -1;

            var choice = available[random.Next(available.Count)];
            return int.Parse(choice);
        }
    }
}