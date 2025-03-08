using Newtonsoft.Json.Linq;

namespace FantasyFootballBot
{
    /// <summary>
    /// Wrapper class that utilizes the <see cref="System.Net.Http.HttpClient"/> class to create calls to the Sleeper API.
    /// </summary>
    public class SleeperClient : HttpClient
    {
        private const string sleeperBaseUri = "https://api.sleeper.app/v1/";
        private const string leagueId = Constants.leagueId;

        public SleeperClient() : base()
        {
            this.BaseAddress = new Uri(sleeperBaseUri);
        }

        public async Task<JObject> GetLeague()
        {
            return JObject.Parse(await this.GetStringAsync($"league/{leagueId}"));
        }

        public async Task<JArray> GetLeagueUsers()
        {
            return JArray.Parse(await this.GetStringAsync($"league/{leagueId}/users"));
        }

        public async Task<JArray> GetLeagueRosters()
        {
            return JArray.Parse(await this.GetStringAsync($"league/{leagueId}/rosters"));
        }

        public async Task<JArray> GetMatchups(int week)
        {
            return JArray.Parse(await this.GetStringAsync($"league/{leagueId}>/matchups/{week}"));
        }

        public async Task<JObject> GetPlayers()
        {
            return JObject.Parse(await this.GetStringAsync("players/nfl"));
        }

        public async Task<JObject> GetNflState()
        {
            return JObject.Parse(await this.GetStringAsync("state/nfl"));
        }
    }
}
