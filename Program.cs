﻿using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using OpenAI.Chat;

namespace FantasyFootballBot
{
    public class Program
    {
        private static DiscordClient? DiscordBotClient { get; set; }
        private static ChatClient? ChatBotClient { get; set; }

        public static async Task Main(string[] args)
        {
            // grab secret values from Key Vault instance
            var kvUrl = $"https://{Constants.keyVaultName}.vault.azure.net";
            var kvClient = new SecretClient(new Uri(kvUrl), new DefaultAzureCredential(
                new DefaultAzureCredentialOptions()
                {
                    ManagedIdentityClientId = Constants.managedIdentityClientId,
                }
            ));

            var aiModelKey1 = (await kvClient.GetSecretAsync(Constants.azureOpenAiKey1Name)).Value.Value;
            var aiModelKey2 = (await kvClient.GetSecretAsync(Constants.azureOpenAiKey2Name)).Value.Value;

            // create Discord Client
            DiscordBotClient = new DiscordClient(new DiscordConfiguration()
            {
                Token = (await kvClient.GetSecretAsync(Constants.discordBotTokenName)).Value.Value,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged.AddIntent(DiscordIntents.MessageContents),
            });

            // create Azure OpenAi Chat Client
            // TODO: add logic to test second key if the first doesn't work
            AzureOpenAIClient aiClient = new AzureOpenAIClient(new Uri(Constants.azureOpenAiEndpoint), new Azure.AzureKeyCredential(aiModelKey1!));
            ChatBotClient = aiClient.GetChatClient(Constants.chatDeploymentName);

            DiscordBotClient.MessageCreated += OnMessageCreated;
            DiscordBotClient.MessageReactionAdded += OnReactionAdded;

            await DiscordBotClient.ConnectAsync();
            await Task.Delay(-1);
        }

        /// <summary>
        /// Method for reacting to a new message sent in the Discord.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs args)
        {
            if (args.Author.Id.Equals(Constants.pooKingUserId)) await args.Message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":poop:", true));

            if (!args.Channel.Name.Equals("paulie-chat")) return;

            var message = args.Message;

            if (message.Content.Substring(0, 22).Contains($"<@{Constants.paulieBotUserId}>"))
            {
                var prompt = message.Content.Substring(23).ToLower();

                var messages = new List<ChatMessage>()
                {
                    new SystemChatMessage("You are a helpful AI Bot."),
                    new UserChatMessage(prompt),
                };

                ChatCompletion chatCompletion = ChatBotClient!.CompleteChat(messages, new ChatCompletionOptions()
                {
                    MaxOutputTokenCount = 150,
                });

                await message.RespondAsync(chatCompletion.Content[0].Text);
            }
        }

        /// <summary>
        /// Method for operations after a reaction is supplied on a message.
        /// </summary>
        /// <param name="sender">Client instance that recieved the reaction event.</param>
        /// <param name="args">Arguments for the event.</param>
        private static async Task OnReactionAdded(DiscordClient sender, MessageReactionAddEventArgs args)
        {
            var messageId = args.Message.Id;
            var message = await args.Channel.GetMessageAsync(messageId);

            var reactCount = 0;
            foreach (DiscordReaction reaction in message.Reactions)
            {
                reactCount += reaction.Count;
                if (reaction.Emoji.GetDiscordName().Equals(":spaghetti:") && reaction.IsMe) return;
            }

            if (reactCount >= 7)
            {
                await new DiscordMessageBuilder()
                    .WithContent("Ah Mamma Mia, that's a-lotta reacts-a!")
                    .WithReply(messageId)
                    .SendAsync(message.Channel);
                await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":spaghetti:", true));
            }
        }
    }
}