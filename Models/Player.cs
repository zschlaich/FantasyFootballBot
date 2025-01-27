using Newtonsoft.Json.Linq;
using static FantasyFootballBot.Constants;

namespace FantasyFootballBot.Models
{
    public class Player
    {
        // Player personal info
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }

        // Fantasy info
        public string PlayerId { get; set; }
        public string TeamCode { get; set; }
        public List<PlayerPositions> Positions { get; set; } = [];

        public Player(JObject playerJson)
        {
            FirstName = (string)playerJson.SelectToken("first_name")!;
            LastName = (string)playerJson.SelectToken("last_name")!;
            FullName = (string)playerJson.SelectToken("full_name")!;
            PlayerId = (string)playerJson.SelectToken("player_id")!;
            TeamCode = (string)playerJson.SelectToken("team")!;

            var fantasyPositions = playerJson.SelectToken("fantasy_positions")!.ToList();
            var playerPositions = new List<PlayerPositions>();
            foreach (var position in fantasyPositions)
            {
                if (Enum.TryParse(position.ToString(), out PlayerPositions playerPosition)) playerPositions.Add(playerPosition);
            }
            Positions = playerPositions;
        }

        public override string ToString()
        {
            return $"'{FullName} ({String.Join(", ", Positions)}), {TeamCode}, {PlayerId}'";
        }
    }
}
