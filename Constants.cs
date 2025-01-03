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

        // Discord info
        public const ulong paulieBotUserId = 1324767587271311410;
        public const ulong pooKingUserId = 695431484604940351;

        // Fantasy Scoring

        // Fantasy League Info
    }
}