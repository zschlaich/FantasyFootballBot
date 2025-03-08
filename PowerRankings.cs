using FantasyFootballBot.Models;
using Newtonsoft.Json.Linq;

namespace FantasyFootballBot
{
    public class PowerRankings
    {
        private readonly SleeperClient sleeperClient;

        /// <summary>
        /// Mapping of key: userId, value: Team.
        /// </summary>
        public Dictionary<string, Team> Teams { get; set; } = [];

        /// <summary>
        /// Mapping of key: userId, value: teamName.
        /// </summary>
        public Dictionary<string, string> TeamNames { get; set; } = [];
        
        public PowerRankings() : this(new SleeperClient())
        {
        }

        public PowerRankings(SleeperClient sleeperClient)
        {
            this.sleeperClient = sleeperClient;
        }

        /// <summary>
        /// Updates the TeamNames dictionary with the current team name.
        /// </summary>
        public async Task UpdateTeamNames()
        {
            var leagueUsersJson = await sleeperClient.GetLeagueUsers();
            foreach (var userJson in leagueUsersJson)
            {
                var user = (JObject)userJson.Value!;
                var userId = (string)user.SelectToken("user_id")!;
                var teamName = (string)user.SelectToken("metadata.team_name")!;

                if (!TeamNames.ContainsKey(userId) || !TeamNames.GetValueOrDefault(userId)!.Equals(teamName))
                {
                    TeamNames.TryAdd(userId, teamName);
                }
            }
        }

        /// <summary>
        /// Updates the Teams dictionary with the current team roster construction.
        /// </summary>
        public async Task UpdateRosters()
        {
            var rostersJson = await sleeperClient.GetLeagueRosters();
            foreach (var rosterJson in rostersJson)
            {
                var roster = (JObject)rosterJson.Value!;
                var ownerId = (string)roster.SelectToken("owner_id")!;
                roster["team_name"] = TeamNames.GetValueOrDefault(ownerId);

                Teams.TryAdd(ownerId, new Team(roster));
            }
            }
        }

        /// <summary>
        /// Generate power rankings for all teams in the league.
        /// </summary>
        public void GenerateRankings()
        {
            throw new NotImplementedException();
        }
    }
}
