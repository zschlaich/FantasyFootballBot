using Newtonsoft.Json.Linq;

namespace FantasyFootballBot.Models
{
    public class Team
    {
        // Team info
        public string LeagueId { get; set; }
        public string OwnerId { get; set; }
        public string Name { get; set; }

        // Team record info
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }

        // Team roster management info
        public int TotalMoves { get; set; }
        public int WaiverBudget { get; set; }
        public int WaiverBudgetUsed { get; set; }
        public int WaiverBudgetRemaining { get; set; }

        // Roster info
        private List<string>? StartersIds;
        private List<string>? BenchIds;
        private List<string>? TaxiIds;
        public List<Player>? Starters { get; set; }
        public List<Player>? Bench { get; set; }
        public List<Player>? Taxi { get; set; }

        // Team scoring info
        public float FptsFor { get; set; }
        public int FptsForWhole { get; set; }
        public int FptsForDecimal { get; set; }
        public float FptsAgainst { get; set; }
        public int FptsAgainstWhole { get; set; }
        public int FptsAgainstDecimal { get; set; }
        public float Ppts { get; set; }
        public int PptsWhole { get; set; }
        public int PptsDecimal { get; set; }

        public Team(JObject teamJson)
        {
            LeagueId = (string)teamJson.SelectToken("league_id")!;
            OwnerId = (string)teamJson.SelectToken("owner_id")!;
            Name = (string)teamJson.SelectToken("team_name")!;

            Wins = (int)teamJson.SelectToken("settings.wins")!;
            Losses = (int)teamJson.SelectToken("settings.losses")!;
            Ties = (int)teamJson.SelectToken("settings.ties")!;

            TotalMoves = (int)teamJson.SelectToken("settings.total_moves")!;
            WaiverBudget = Constants.waiverBudget;
            WaiverBudgetUsed = (int)teamJson.SelectToken("settings.waiver_budget_used")!;
            WaiverBudgetRemaining = WaiverBudget - WaiverBudgetUsed;

            UpdateRoster(teamJson);

            FptsForWhole = (int)teamJson.SelectToken("settings.fpts")!;
            FptsForDecimal = (int)teamJson.SelectToken("settings.fpts_decimal")!;
            FptsFor = (float)FptsForWhole + (float)FptsForDecimal / 100;

            FptsAgainstWhole = (int)teamJson.SelectToken("settings.fpts_against")!;
            FptsAgainstDecimal = (int)teamJson.SelectToken("settings.fpts_against_decimal")!;
            FptsAgainst = (float)FptsAgainstWhole + (float)FptsAgainstDecimal / 100;

            PptsWhole = (int)teamJson.SelectToken("settings.ppts")!;
            PptsDecimal = (int)teamJson.SelectToken("settings.ppts_decimal")!;
            Ppts = (float)PptsWhole + (float)PptsDecimal / 100;
        }

        /// <summary>
        /// Update's a team's roster to include new players and where they are located on the current roster (starter/bench/taxi).
        /// </summary>
        public void UpdateRoster(JObject teamJson)
        {
            StartersIds = teamJson.SelectToken("starters")!.ToObject<List<string>>()!;
            TaxiIds = teamJson.SelectToken("taxi")!.ToObject<List<string>>()!;
            var players = teamJson.SelectToken("players")!.ToObject<List<string>>()!;
            BenchIds = [];
            foreach (var playerId in players)
            {
                if (!StartersIds.Contains(playerId))
                {
                    BenchIds.Add(playerId);
                }
            };
        }

        public override string ToString()
        {
            return $"Team name: '{Name}', Owner id: '{OwnerId}'";
        }
    }
}
