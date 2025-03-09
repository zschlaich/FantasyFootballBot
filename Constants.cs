namespace FantasyFootballBot
{
    public class Constants
    {
        // Azure info
        public const string subscriptionId = "5b1b1dab-48c3-4e76-8419-ff4dfd348984";
        public const string azureOpenAiEndpoint = "https://fantasyfootball-ai.openai.azure.com/";
        public const string chatDeploymentName = "gpt-4o-mini-fantasybot";
        public const string managedIdentityClientId = "51c5201f-1803-453a-ad4c-5eec5e6cd1bd";

        // Key Vault
        public const string keyVaultName = "fantasybotkv";
        public const string azureOpenAiKey1Name = "fantasyfootball-ai-key1";
        public const string azureOpenAiKey2Name = "fantasyfootball-ai-key2";
        public const string discordBotTokenName = "paulie-bot-token";

        // Storage Account
        public const string storageAccountName = "pauliesffstorage";
        public const string storageAccountEndpoint = $"https://{storageAccountName}.blob.core.windows.net";
        public const string rankingsContainerName = "powerrankings";
        public const string playersContainerName = "playerinfo";
        public const string statsContainerName = "fantasydata";
        public const string playersBlobName = "playerJson";

        // Stats GitHub info
        public const string statsOwner = "hvpkod";
        public const string statsRepoName = "NFL-Data";

        // Discord info
        public const ulong paulieBotUserId = 1324767587271311410;
        public const ulong champUserId = benUserId;
        public const ulong pooKingUserId = hunterUserId;
        public const ulong aaronUserId = 1265336422529892392;
        public const ulong andyUserId = 356174235816820736;
        public const ulong benUserId = 354778456762089482;
        public const ulong danUserId = 583745755890843648;
        public const ulong dbrickUserId = 436276711261208587;
        public const ulong hunterUserId = 695431484604940351;
        public const ulong jackUserId = 193441946918715403;
        public const ulong mattUserId = 841082778467565609;
        public const ulong markUserId = 292537478244728833;
        public const ulong patrickUserId = 1247386553400164426;
        public const ulong rickUserId = 543462592627212301;
        public const ulong shawnUserId = 201467329488486400;
        public const ulong zachUserId = 198181459746357248;
        public List<ulong> everyoneId = new()
        {
            aaronUserId,
            andyUserId,
            benUserId,
            danUserId,
            dbrickUserId,
            hunterUserId,
            jackUserId,
            mattUserId,
            markUserId,
            patrickUserId,
            rickUserId,
            shawnUserId,
            zachUserId,
        };

        // Fantasy Scoring
        // TODO look into grabbing this information directly from Sleeper in the future.
        // Passing
        public const float passYard = 0.04F;
        public const float passTd = 4.0F;
        public const float pass2ptConversion = 2.0F;
        public const float passInt= -2.0F;
        // Rushing
        public const float rushYard = 0.1F;
        public const float rushTd = 6.0F;
        public const float rush2ptConversion = 2.0F;
        public const float rushFumbleLost = -2.0F;
        // Receiving
        public const float recReception = 0.5F;
        public const float recYard = 0.1F;
        public const float rec2ptConversion = 2.0F;
        public const float recTd = 6.0F;
        // Defense
        // Kicking

        // Fantasy League Info
        public const string leagueId = "1089364456892686336";
        public const int waiverBudget = 100;

        // Fantasy Football values
        public enum PlayerPositions
        {
            QB,
            RB,
            WR,
            TE,
            K,
            DEF,
            DB,
            DL,
            LB,
        }
        public enum RosterPositions
        {
            QB,
            RB,
            WR,
            TE,
            FLEX,
            SUPER_FLEX,
            BN,
            IR,
            TAXI,
        }
    }
}