using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FantasyFootballBot.Models;
using Newtonsoft.Json.Linq;

namespace FantasyFootballBot
{
    public class PowerRankings
    {
        private readonly SleeperClient sleeperClient;

        private readonly BlobServiceClient rankingsBlobServiceClient;
        private readonly BlobContainerClient rankingsBlobContainerClient;
        private BlobClient? rankingsBlobClient;

        private readonly BlobServiceClient playerBlobServiceClient;
        private readonly BlobContainerClient playerBlobContainerClient;
        private readonly BlobClient playerBlobClient;

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

            var storageAccountEndpoint = Constants.storageAccountEndpoint;
            var azureCredential = new DefaultAzureCredential(
                new DefaultAzureCredentialOptions()
                {
                    ManagedIdentityClientId = Constants.managedIdentityClientId,
        }
            );

            rankingsBlobServiceClient = new BlobServiceClient(new Uri(storageAccountEndpoint), azureCredential);
            rankingsBlobContainerClient = rankingsBlobServiceClient.GetBlobContainerClient(Constants.rankingsContainerName);

            playerBlobServiceClient = new BlobServiceClient(new Uri(storageAccountEndpoint), azureCredential);
            playerBlobContainerClient = playerBlobServiceClient.GetBlobContainerClient(Constants.playersContainerName);
            playerBlobClient = playerBlobContainerClient.GetBlobClient(Constants.playersBlobName);
        }

        /// <summary>
        /// Updates the TeamNames dictionary with the current team name.
        /// </summary>
        public async Task UpdateTeamNames()
        {
            var leagueUsersJson = await sleeperClient.GetLeagueUsers();
            foreach (var userJson in leagueUsersJson)
            {
                var user = (JObject)userJson;
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
            var playersJson = await GetPlayers();
            foreach (var rosterJson in rostersJson)
            {
                var roster = (JObject)rosterJson;
                var ownerId = (string)roster.SelectToken("owner_id")!;
                roster.Add("team_name", TeamNames.GetValueOrDefault(ownerId));

                var team = new Team(roster);
                team.UpdateRoster(playersJson);

                Teams.TryAdd(ownerId, team);
            }
        }

        /// <summary>
        /// Get the JSON representation of NFL player data from Sleeper.
        /// </summary>
        /// <returns><see cref="JObject"/> representing NFL player data from Sleeper.</returns>
        private async Task<JObject> GetPlayers()
        {
            await UpdatePlayers();
            BlobDownloadResult result = await playerBlobClient.DownloadContentAsync();
            return JObject.Parse(result.Content.ToString());
            }

        /// <summary>
        /// Updates the cached active player list. Data is only modified once per day to reduce calls made to the Sleeper API.
        /// </summary>
        private async Task UpdatePlayers()
        {
            BlobDownloadResult result = await playerBlobClient.DownloadContentAsync();
            if (new DateTimeOffset(DateTime.Now).CompareTo(result.Details.LastModified.AddDays(1)) >= 0)
            {
                var playersJson = await sleeperClient.GetPlayers();
                await playerBlobClient.UploadAsync(BinaryData.FromString(playersJson.ToString()), overwrite: true);
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
