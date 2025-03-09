using System.Text.RegularExpressions;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using FantasyFootballBot.Clients;
using OpenAI.Chat;

namespace FantasyFootballBot
{
    public class Program
    {
        private static DiscordClient? DiscordBotClient { get; set; }
        private static ChatClient? ChatBotClient { get; set; }
        private static StatsClient? NflStatsClient { get; set; }
        private static PowerRankings? PauliesPowerRankings { get; set; }

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

            // create PowerRankings object
            PauliesPowerRankings = new PowerRankings();

            // create StatsClient
            NflStatsClient = new StatsClient();

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
            await AddReactions(args);

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

            var reactedUsers = new HashSet<DiscordUser>();
            foreach (DiscordReaction reaction in message.Reactions)
            {
                if (reaction.Emoji.GetDiscordName().Equals(":spaghetti:") && reaction.IsMe) return;

                foreach (DiscordUser user in await message.GetReactionsAsync(reaction.Emoji))
                {
                    reactedUsers.Add(user);
                }
            }

            if (reactedUsers.Count >= 8)
            {
                await new DiscordMessageBuilder()
                    .WithContent("Ah Mamma Mia, that's a-lotta reacts-a!")
                    .WithReply(messageId)
                    .SendAsync(message.Channel);
                await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":spaghetti:", true));
            }
        }

        /// <summary>
        /// Add reactions to messages based on specific users or phrases.
        /// </summary>
        /// <param name="args">MessageCreateEventArgs for the message that was just sent.</param>
        private static async Task AddReactions(MessageCreateEventArgs args)
        {
            var message = args.Message;
            var messageContent = message.Content.ToLower();

            // champion trophy reaction
            if (args.Author.Id.Equals(Constants.champUserId)) await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":trophy:"));

            // poo king poo reaction
            if (args.Author.Id.Equals(Constants.pooKingUserId)) await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":poop:"));

            // random trump reaction
            if (messageContent.Contains("trump"))
            {
                var randInt = new Random().Next();
                if (randInt % 2 == 0)
                {
                    await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":trump:", true));
                }
                else
                {
                    await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":cheekybastard:", true));
                }
            }

            // maryland pride reactions
            if (messageContent.Contains("maryland") || messageContent.Contains("umd") || messageContent.Contains("md") || messageContent.Contains("terp") || messageContent.Contains("terps"))
                await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":terps:", true));

            // clown reaction
            if (messageContent.Contains("clown") || messageContent.Contains("cloen"))
                await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":jimbo:", true));

            // add emoji reaction for direct @'s
            var pattern = @"<@(\d+)>";
            var regex = new Regex(pattern);
            var matches = regex.Matches(messageContent);
            if (matches.Count != 0)
            {
                var mentionedIds = new List<ulong>();
                foreach (Match match in matches)
                {
                    foreach (Capture capture in match.Groups[1].Captures)
                    {
                        mentionedIds.Add(UInt64.Parse(capture.ToString()));
                    }
                }

                foreach (var id in mentionedIds)
                {
                    switch (id)
                    {
                        case Constants.paulieBotUserId:
                            await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":paulie:", true));
                            break;
                        case Constants.aaronUserId:
                            await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":aaron:", true));
                            break;
                        case Constants.andyUserId:
                            await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":andy:", true));
                            break;
                        case Constants.benUserId:
                            await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":ben:", true));
                            break;
                        case Constants.danUserId:
                            await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":dan:", true));
                            break;
                        case Constants.dbrickUserId:
                            await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":dbrick:", true));
                            break;
                        case Constants.hunterUserId:
                            await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":hunter:", true));
                            break;
                        case Constants.jackUserId:
                            await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":jack:", true));
                            break;
                        case Constants.markUserId:
                            await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":mark:", true));
                            break;
                        case Constants.mattUserId:
                            await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":matt:", true));
                            break;
                        case Constants.patrickUserId:
                            await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":patrick:", true));
                            break;
                        case Constants.rickUserId:
                            await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":rick:", true));
                            break;
                        case Constants.shawnUserId:
                            await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":shawn:", true));
                            break;
                        case Constants.zachUserId:
                            await message.CreateReactionAsync(DiscordEmoji.FromName(DiscordBotClient, ":zach:", true));
                            break;
                    }
                }
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