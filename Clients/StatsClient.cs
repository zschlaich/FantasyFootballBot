using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Newtonsoft.Json.Linq;
using Octokit;
using static FantasyFootballBot.Constants;

namespace FantasyFootballBot.Clients
{
    /// <summary>
    /// Client responsible for copying and formatting stats information from source Github repo to our Storage Account.
    /// </summary>
    class StatsClient
    {
        private readonly GitHubClient _gitHubClient;
        private readonly BlobServiceClient _statsBlobServiceClient;
        private readonly BlobContainerClient _statsBlobContainerClient;

        private readonly string _finalStatsPathWeek = "NFL-data-Players/{0}/{1}/{2}.json";
        private readonly string _finalStatsPathSeason = "NFL-data-Players/{0}/{1}_season.json";
        private readonly string _projectedStatsPath = "NFL-data-Players/{0}/{1}/projected/{2}_projected.json";

        private readonly List<PlayerPositions> _desiredPositions = 
        [
            PlayerPositions.QB,
            PlayerPositions.RB,
            PlayerPositions.WR,
            PlayerPositions.TE,
        ];

        public StatsClient()
        {
            _gitHubClient = new GitHubClient(new ProductHeaderValue("FantasyFootballBot"));

            var storageAccountEndpoint = Constants.storageAccountEndpoint;
            var azureCredential = new DefaultAzureCredential(
                new DefaultAzureCredentialOptions()
                {
                    ManagedIdentityClientId = managedIdentityClientId,
                }
            );

            _statsBlobServiceClient = new BlobServiceClient(new Uri(storageAccountEndpoint), azureCredential);
            _statsBlobContainerClient = _statsBlobServiceClient.GetBlobContainerClient(statsContainerName);
        }

        /// <summary>
        /// Grab the final stats for all desired positions for a specified week during a specified season year. This information is then uploaded to the Storage Account.
        /// </summary>
        /// <param name="year">Season year.</param>
        /// <param name="week">Season week.</param>
        public async Task AddFinalStatsWeek(int year, int week)
        {
            foreach (PlayerPositions position in _desiredPositions)
            {
                await GetFinalStatsWeek(year, week, position);
            }
        }

        /// <summary>
        /// Grab the projected stats for all desired positions for a specified week during a specified season year. This information is then uploaded to the Storage Account.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="week"></param>
        public async Task AddProjectedStatsWeek(int year, int week)
        {
            foreach (PlayerPositions position in _desiredPositions)
            {
                await GetProjectedStatsWeek(year, week, position);
            }
        }

        /// <summary>
        /// Grab the final stats for all desired positions for a specified season year. This information is then uploaded to the Storage Account.
        /// </summary>
        /// <param name="year"></param>
        public async Task AddFinalStatsSeason(int year)
        {
            foreach (PlayerPositions position in _desiredPositions)
            {
                await GetFinalStatsSeason(year, position);
            }
        }

        private async Task GetFinalStatsWeek(int year, int week, PlayerPositions position)
        {
            var posPath = string.Format(_finalStatsPathWeek, year, week, position);
            var weekStatsList = await _gitHubClient.Repository.Content.GetAllContents(statsOwner, statsRepoName, posPath);

            var encodedStats = weekStatsList[0].EncodedContent;
            byte[] data = Convert.FromBase64String(encodedStats);
            var decodedStatsString = System.Text.Encoding.UTF8.GetString(data);

            var statsJson = JArray.Parse(decodedStatsString);
            await UploadStats($"{year}/{week}/{position}.json", statsJson);
        }

        private async Task GetProjectedStatsWeek(int year, int week, PlayerPositions position)
        {
            var posPath = string.Format(_projectedStatsPath, year, week, position);
            var weekStatsList = await _gitHubClient.Repository.Content.GetAllContents(statsOwner, statsRepoName, posPath);

            var encodedStats = weekStatsList[0].EncodedContent;
            byte[] data = Convert.FromBase64String(encodedStats);
            var decodedStatsString = System.Text.Encoding.UTF8.GetString(data);

            var statsJson = JArray.Parse(decodedStatsString);
            await UploadStats($"{year}/{week}/projected/{position}_projected.json", statsJson);
        }

        private async Task GetFinalStatsSeason(int year, PlayerPositions position)
        {
            var posPath = string.Format(_finalStatsPathSeason, year, position);
            var seasonStatsList = await _gitHubClient.Repository.Content.GetAllContents(statsOwner, statsRepoName, posPath);

            var encodedStats = seasonStatsList[0].EncodedContent;
            byte[] data = Convert.FromBase64String(encodedStats);
            var decodedStatsString = System.Text.Encoding.UTF8.GetString(data);

            var statsJson = JArray.Parse(decodedStatsString);
            await UploadStats($"{year}/{position}_season.json", statsJson);
        }

        private async Task UploadStats(string path, JToken data)
        {
            try
            {
                await _statsBlobContainerClient.UploadBlobAsync(path, BinaryData.FromString(data.ToString()));
            }
            catch (RequestFailedException)
            {
                // File already uploaded, skip this instance.
            }
        }
    }
}
