using FantasyFootballBot.Models;
using Newtonsoft.Json.Linq;

namespace FantasyFootballBot
{
    public class PowerRankings
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// Mapping of key: userId, value: Team.
        /// </summary>
        public Dictionary<string, Team> Teams { get; set; } = [];

        /// <summary>
        /// Mapping of key: userId, value: teamName.
        /// </summary>
        public Dictionary<string, string> TeamNames { get; set; } = [];
        
        public PowerRankings()
        {
            httpClient = new HttpClient();
        }

        public PowerRankings(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Updates the TeamNames dictionary with the current team name.
        /// </summary>
        public async void UpdateTeams()
        {
            var leagueUsers = await httpClient.GetStringAsync($"https://api.sleeper.app/v1/league/{Constants.leagueId}/users");
            var leagueUsersJson = JObject.Parse(leagueUsers);
            foreach (var userJson in leagueUsersJson)
            {
                var user = (JObject)userJson.Value!;
                var userId = (string)user.SelectToken("user_id")!;
                var teamName = (string)user.SelectToken("metadata.team_name")!;

                if (!TeamNames.ContainsKey(userId) || !TeamNames.GetValueOrDefault(userId)!.Equals(teamName))
                {
                    TeamNames.Add(userId, teamName);
                }
            }
        }

        /// <summary>
        /// Updates the Teams dictionary with the current team roster construction.
        /// </summary>
        public async void UpdateRosters()
        {
            var rosters = await httpClient.GetStringAsync($"https://api.sleeper.app/v1/league/{Constants.leagueId}/rosters");
            var rostersJson = JObject.Parse(rosters);
            foreach (var rosterJson in rostersJson)
            {
                var roster = (JObject)rosterJson.Value!;
                var ownerId = (string)roster.SelectToken("owner_id")!;
                roster["team_name"] = TeamNames.GetValueOrDefault(ownerId);

                Teams.Add(ownerId, new Team(roster));
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
