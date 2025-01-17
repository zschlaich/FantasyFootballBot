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
        /// <param name="sender">Client instance that recieved the message event.</param>
        /// <param name="args">Arguments for the event.</param>
        private static async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs args)
        {
            if (args.Author.Id.Equals(Constants.pooKingUserId)) await args.Message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":poop:", true));

            var allowedChannels = new List<string>()
            {
                "paulie-chat",
                "bot-test",
            };

            if (!allowedChannels.Contains(args.Channel.Name)) return;

            var message = args.Message;
            var channel = args.Channel;

            // Paulie is only prompted when the message starts with "@Paulie"
            if (message.Content.StartsWith($"<@{Constants.paulieBotUserId}>"))
            {
                // check if message only contains "@Paulie"
                if (message.Content.Equals($"<@{Constants.paulieBotUserId}>"))
                {
                    await message.RespondAsync("I'm sorry, did you mean to ask me something?");
                    return;
                }

                var previousChats = new List<ChatMessage>();
                // check if message is part of a previous conversation
                if (message.ReferencedMessage != null)
                {
                    await ConversationHelper(previousChats, channel, message.ReferencedMessage.Id);
                }

                var prompt = message.Content.Substring(23).ToLower();

                var messages = new List<ChatMessage>() { new SystemChatMessage("You are a helpful AI Bot.") };
                messages.AddRange(previousChats);
                messages.Add(new UserChatMessage(prompt));

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

        /// <summary>
        /// Helper method used to recursively collect all previous messages sent in a conversation.
        /// </summary>
        /// <param name="messages">List of ChatMessage objects previously sent in this conversation.</param>
        /// <param name="channel">Channel this conversation is occurring in.</param>
        /// <param name="messageId">Message ID value of the message we are currently adding to the messages object.</param>
        private static async Task<List<ChatMessage>> ConversationHelper(List<ChatMessage> messages, DiscordChannel channel, ulong messageId)
        {
            var message = await channel.GetMessageAsync(messageId);

            if (message.Author.IsCurrent) messages.Insert(0, new AssistantChatMessage(message.Content));
            else messages.Insert(0, new UserChatMessage(message.Content.Substring(23).ToLower()));

            if (message.ReferencedMessage != null)
            {
                await ConversationHelper(messages, channel, message.ReferencedMessage.Id);
            }

            return messages;
        }
    }
}