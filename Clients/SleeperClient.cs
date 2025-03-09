using Newtonsoft.Json.Linq;

namespace FantasyFootballBot.Clients
{
    /// <summary>
    /// Wrapper class that utilizes the <see cref="HttpClient"/> class to create calls to the Sleeper API.
    /// </summary>
    public class SleeperClient : HttpClient
    {
        private const string sleeperBaseUri = "https://api.sleeper.app/v1/";
        private const string leagueId = Constants.leagueId;

        public SleeperClient() : base()
        {
            BaseAddress = new Uri(sleeperBaseUri);
        }

        public async Task<JObject> GetLeague()
        {
            return JObject.Parse(await GetStringAsync($"league/{leagueId}"));
        }

        public async Task<JArray> GetLeagueUsers()
        {
            return JArray.Parse(await GetStringAsync($"league/{leagueId}/users"));
        }

        public async Task<JArray> GetLeagueRosters()
        {
            return JArray.Parse(await GetStringAsync($"league/{leagueId}/rosters"));
        }

        public async Task<JArray> GetMatchups(int week)
        {
            return JArray.Parse(await GetStringAsync($"league/{leagueId}>/matchups/{week}"));
        }

        public async Task<JObject> GetPlayers()
        {
            return JObject.Parse(await GetStringAsync("players/nfl"));
        }

        public async Task<JObject> GetNflState()
        {
            return JObject.Parse(await GetStringAsync("state/nfl"));
        }
    }
}
