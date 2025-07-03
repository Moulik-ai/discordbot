using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

namespace discordbot.commands
{
    public class TestCommands : ApplicationCommandModule
    {
        [SlashCommand("ping", "Replies with Pong!")]
        public async Task PingCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Pong!"));
        }

        [SlashCommand("hello", "Greets the user.")]
        public async Task HelloCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Hello, {ctx.User.Mention}!"));
        }

        [SlashCommand("yo", "Says 'sup' to the user.")]
        public async Task YoCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"sup, {ctx.User.Mention}!"));
        }

        [SlashCommand("test", "Checks if the test command works.")]
        public async Task Test(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Test command works!"));
        }

        [SlashCommand("online", "Checks if the bot is online.")]
        public async Task OnlineCommand(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Online Status",
                Description = "The bot is online and functioning correctly.",
                Color = DiscordColor.Green
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("repeat", "Repeats your message.")]
        public async Task RepeatCommand(InteractionContext ctx, [Option("message", "The message to repeat")] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Please provide a message to repeat."));
                return;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(message));
        }

        [SlashCommand("kick", "Kicks a member from the server.")]
        public async Task KickCommand(InteractionContext ctx, [Option("member", "The member to kick")] DiscordUser user)
        {
            var member = ctx.Guild.GetMemberAsync(user.Id).Result;
            var invoker = ctx.Guild.GetMemberAsync(ctx.User.Id).Result;

            if (invoker.Permissions.HasPermission(Permissions.KickMembers))
            {
                await member.RemoveAsync("Kicked by slash command");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{member.Username} has been kicked."));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You do not have permission to kick members."));
            }
        }

        [SlashCommand("invite", "Get the bot's invite link.")]
        public async Task InviteCommand(InteractionContext ctx)
        {
            var inviteLink = "https://discord.com/api/oauth2/authorize?client_id=1387283422283567194&permissions=8&scope=bot";
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"You can invite me using this link: {inviteLink}"));
        }

        [SlashCommand("ban", "Bans a member from the server.")]
        public async Task BanCommand(InteractionContext ctx, [Option("member", "The member to ban")] DiscordUser user)
        {
            DiscordMember member = null;
            DiscordMember invoker = null;
            try
            {
                member = await ctx.Guild.GetMemberAsync(user.Id);
                invoker = await ctx.Guild.GetMemberAsync(ctx.User.Id);
            }
            catch
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Could not find the specified member."));
                return;
            }

            if (invoker.Permissions.HasPermission(Permissions.BanMembers))
            {
                await member.BanAsync(0, "Banned by slash command");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{member.Username} has been banned."));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You do not have permission to ban members."));
            }
        }

        [SlashCommand("Support", "Provides support information.")]
        public async Task SupportCommand(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Support Information",
                Description = "If you need help, please join our support server: [Support Server](https://discord.gg/sbWXxzzHqw)",
                Color = DiscordColor.Blue
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("clear", "Clears a specified number of messages from the channel.")]
        public async Task ClearCommand(InteractionContext ctx, [Option("count", "Number of messages to clear")] long count)
        {
            if (count <= 0 || count > 1000)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Please specify a number between 1 and 1000."));
                return;
            }

            var messages = await ctx.Channel.GetMessagesAsync((int)count);
            await ctx.Channel.DeleteMessagesAsync(messages);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{messages.Count} messages cleared."));
        }


        [SlashCommand("userinfo", "Displays information about a user.")]
        public async Task UserInfoCommand(InteractionContext ctx, [Option("user", "The user to get information about")] DiscordUser user)
        {
            if (user == null)
                user = ctx.User; // If no user is specified, use the command invoker

            DiscordMember member = null;
            try
            {
                member = await ctx.Guild.GetMemberAsync(user.Id);
            }
            catch
            {
                // User is not a member of the guild
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{user.Username}'s Information",
                Description = $"ID: {user.Id}\n" +
                          (member != null ? $"Joined Server: {member.JoinedAt.ToString("f")}\n" : "") +
                          $"Account Created: {user.CreationTimestamp.ToString("f")}\n" +
                          (member != null ? $"Roles: {string.Join(", ", member.Roles.Select(r => r.Name))}" : "Roles: N/A (not a server member)"),
                Color = DiscordColor.Gold
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("serverinfo", "Displays information about the server.")]
        public async Task ServerInfoCommand(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"{ctx.Guild.Name} Server Information",
                Description = $"ID: {ctx.Guild.Id}\n" +
                              $"Created On: {ctx.Guild.CreationTimestamp.ToString("f")}\n" +
                              $"Member Count: {ctx.Guild.MemberCount}\n" +
                              $"Owner: {ctx.Guild.Owner.Username}#{ctx.Guild.Owner.Discriminator}",
                Color = DiscordColor.Blue
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("unban", "Unbans a member from the server.")]
        public async Task UnbanCommand(InteractionContext ctx, [Option("user_id", "The ID of the user to unban")] long userId)
        {
            DiscordMember invoker = null;
            try
            {
                invoker = await ctx.Guild.GetMemberAsync(ctx.User.Id);
            }
            catch
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Could not get your member information."));
                return;
            }

            if (invoker.Permissions.HasPermission(Permissions.BanMembers))
            {
                try
                {
                    await ctx.Guild.UnbanMemberAsync((ulong)userId);
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"User with ID {userId} has been unbanned."));
                }
                catch (Exception ex)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Failed to unban user: {ex.Message}"));
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You do not have permission to unban members."));
            }
        }

        [SlashCommand("avatar", "Displays the avatar of a user.")]
        public async Task AvatarCommand(InteractionContext ctx, [Option("user", "The user to get the avatar of")] DiscordUser user)
        {
            if (user == null)
                user = ctx.User; // If no user is specified, use the command invoker

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{user.Username}'s Avatar",
                ImageUrl = user.AvatarUrl,
                Color = DiscordColor.Purple
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        // Dictionary to store the last deleted message per channel
        private static readonly Dictionary<ulong, (string Content, DiscordUser Author)> _lastDeletedMessages = new();

        // This method should be called from your main bot setup, not here:
        // client.MessageDeleted += OnMessageDeletedAsync;

        public static async Task OnMessageDeletedAsync(DiscordClient client, DSharpPlus.EventArgs.MessageDeleteEventArgs e)
        {
            if (e.Message == null || string.IsNullOrWhiteSpace(e.Message.Content))
                return;

            _lastDeletedMessages[e.Channel.Id] = (e.Message.Content, e.Message.Author);
        }

        [SlashCommand("snipe", "Shows the last deleted message in this channel.")]
        public async Task SnipeCommand(InteractionContext ctx)
        {
            if (Program.SnipedMessages.TryGetValue(ctx.Channel.Id, out var snipe))
            {
                var embed = new DiscordEmbedBuilder()
                    .WithAuthor(snipe.Author.Username, iconUrl: snipe.Author.AvatarUrl)
                    .WithDescription(snipe.Content)
                    .WithColor(DiscordColor.Orange)
                    .WithFooter("Sniped by " + ctx.User.Username);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No messages to snipe."));
            }
        }

        [SlashCommand("emojify", "Convert texts into emoji letters or fun unicode art.")]
        public async Task EmojifyCommand(InteractionContext ctx, [Option("text", "The text to emojify")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Please provide some text to emojify."));
                return;
            }

            var emojified = new StringBuilder();
            foreach (char c in text.ToLower())
            {
                if (c >= 'a' && c <= 'z')
                    emojified.Append($":regional_indicator_{c}:");
                else if (char.IsDigit(c))
                    emojified.Append(GetNumberEmoji(c));
                else if (char.IsWhiteSpace(c))
                    emojified.Append("  ");
                else
                    emojified.Append(c);
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(emojified.ToString()));
        }
        // Helper method for number emojis
        private string GetNumberEmoji(char digit)
        {
            return digit switch
            {
                '0' => ":zero:",
                '1' => ":one:",
                '2' => ":two:",
                '3' => ":three:",
                '4' => ":four:",
                '5' => ":five:",
                '6' => ":six:",
                '7' => ":seven:",
                '8' => ":eight:",
                '9' => ":nine:",
                _ => digit.ToString()
            };
        }

        [SlashCommand("roll", "Rolls a dice with the specified number of sides.")]
        public async Task RollCommand(InteractionContext ctx, [Option("sides", "Number of sides on the dice")] long sides)
        {
            // Provide a default if not specified
            if (sides == 0)
                sides = 6;

            if (sides < 1)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The number of sides must be at least 1."));
                return;
            }

            var random = new Random();
            int result = random.Next(1, (int)sides + 1);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"You rolled a {result} on a {sides}-sided dice."));
        }

        [SlashCommand("say", "Makes the bot say something.")]
        public async Task SayCommand(InteractionContext ctx, [Option("message", "The message for the bot to say")] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Please provide a message for me to say."));
                return;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(message));
        }

        [SlashCommand("afk", "Set your AFK status with a custom message.")]
        public async Task AfkCommand(InteractionContext ctx, [Option("message", "Your AFK message (optional)")] string message = "AFK")
        {
            // Store the AFK status in a way that can be retrieved later
            // This could be a database, in-memory cache, etc.
            // For simplicity, we'll just use a static dictionary here
            var afkStatus = new Dictionary<ulong, string>
            {
                { ctx.User.Id, message }
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"You are now AFK: {message}"));
        }

        [SlashCommand("unafk", "Remove your AFK status.")]
        public async Task UnafkCommand(InteractionContext ctx)
        {
            // Remove the AFK status from the storage
            // This should match how you stored it in the AfkCommand
            var afkStatus = new Dictionary<ulong, string>();
            if (afkStatus.ContainsKey(ctx.User.Id))
            {
                afkStatus.Remove(ctx.User.Id);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You are no longer AFK."));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You were not AFK."));
            }
        }

        [SlashCommand("poll", "Create a quick poll with custom options for server voting")]
        public async Task PollCommand(InteractionContext ctx,
            [Option("question", "The question for the poll")] string question,
            [Option("option1", "First option for the poll")] string option1,
            [Option("option2", "Second option for the poll")] string option2,
            [Option("option3", "Third option for the poll (optional)")] string option3 = null,
            [Option("option4", "Fourth option for the poll (optional)")] string option4 = null,
            [Option("option5", "Fifth option for the poll (optional)")] string option5 = null,
            [Option("duration", "Duration of the poll in minutes (default is 5 minutes)")] long duration = 10)
        {
            // Provide defaults inside the method
            if (duration <= 0) duration = 10;
            var options = new List<string> { option1, option2 };
            if (!string.IsNullOrWhiteSpace(option3)) options.Add(option3);
            if (!string.IsNullOrWhiteSpace(option4)) options.Add(option4);
            if (!string.IsNullOrWhiteSpace(option5)) options.Add(option5);


            var embed = new DiscordEmbedBuilder
            {
                Title = "Poll",
                Description = question,
                Color = DiscordColor.Green
            };

            foreach (var option in options)
            {
                embed.AddField("Option", option);
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));

            // Add reactions for voting
            var reactions = new[] { "👍", "👎" };
            var sentMessage = await ctx.GetOriginalResponseAsync();
            foreach (var reaction in reactions)
            {
                var emoji = DiscordEmoji.FromUnicode(ctx.Client, reaction);
                await sentMessage.CreateReactionAsync(emoji);
            }
        }

        [SlashCommand("timer", "Start a countdown timer in the channel.")]
        public async Task TimerCommand(InteractionContext ctx, [Option("duration", "Duration of the timer in seconds")] long duration)
        {
            if (duration <= 0)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Please specify a duration greater than 0 seconds."));
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = "Timer Started",
                Description = $"A timer for {duration} seconds has been started.",
                Color = DiscordColor.Orange
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));

            // Start the timer
            await Task.Delay((int)(duration * 1000));

            // Notify when the timer ends
            await ctx.Channel.SendMessageAsync($"⏰ Timer ended after {duration} seconds!");
        }


        [SlashCommand("timeout", "Timeout a member for a specified duration.")]
        public async Task TimeoutCommand(InteractionContext ctx, [Option("member", "The member to timeout")] DiscordUser user, [Option("duration", "Duration of the timeout in minutes")] long duration)
        {
            if (duration <= 0)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Please specify a duration greater than 0 minutes."));
                return;
            }

            var member = await ctx.Guild.GetMemberAsync(user.Id);
            var invoker = await ctx.Guild.GetMemberAsync(ctx.User.Id);

            if (member == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Member not found in this server."));
                return;
            }

            // Check if the target is the server owner
            if (member.Id == ctx.Guild.OwnerId)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You cannot timeout the server owner."));
                return;
            }

            // Check role hierarchy: bot vs target
            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (botMember.Hierarchy <= member.Hierarchy)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("I cannot timeout this user because their role is higher or equal to mine."));
                return;
            }

            // Check role hierarchy: invoker vs target
            if (invoker.Hierarchy <= member.Hierarchy && invoker.Id != ctx.Guild.OwnerId)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You cannot timeout this user because their role is higher or equal to yours."));
                return;
            }

            // Apply the timeout
            var timeoutEnd = DateTimeOffset.UtcNow.AddMinutes(duration);
            await member.TimeoutAsync(timeoutEnd, "Timeout by slash command");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{user.Username} has been timed out for {duration} minutes."));
        }

        [SlashCommand("goat", "Replies with the greatest player of all time.")]
        public async Task GreatestPlayerCommand(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Greatest Player of All Time",
                Description = "The greatest player of all time is **Lionel Messi**.",
                Color = DiscordColor.Blue
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }



        [SlashCommand("weather", "Get the current weather for a city.")]
        public async Task WeatherCommand(InteractionContext ctx, [Option("city", "The city to get weather for")] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Please provide a city name."));
                return;
            }

            string apiKey = "aba46820876f579ffbbb17b6a825d6ec"; // <-- Replace with your OpenWeatherMap API key
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(city)}&appid={apiKey}&units=metric";

            using var http = new HttpClient();
            try
            {
                var response = await http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Could not find weather data for that city."));
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                string weather = root.GetProperty("weather")[0].GetProperty("description").GetString();
                double temp = root.GetProperty("main").GetProperty("temp").GetDouble();
                double feelsLike = root.GetProperty("main").GetProperty("feels_like").GetDouble();
                int humidity = root.GetProperty("main").GetProperty("humidity").GetInt32();
                double wind = root.GetProperty("wind").GetProperty("speed").GetDouble();

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Weather in {city}",
                    Description = $"**{weather}**\nTemperature: {temp}°C (feels like {feelsLike}°C)\nHumidity: {humidity}%\nWind: {wind} m/s",
                    Color = DiscordColor.Azure
                };

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Error fetching weather: {ex.Message}"));
            }
        }

        [SlashCommand("joke", "Get a random joke.")]
        public async Task JokeCommand(InteractionContext ctx)
        {
            string apiUrl = "https://official-joke-api.appspot.com/random_joke";

            using var http = new HttpClient();
            try
            {
                var response = await http.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Could not fetch a joke at the moment."));
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var setup = doc.RootElement.GetProperty("setup").GetString();
                var punchline = doc.RootElement.GetProperty("punchline").GetString();

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{setup}\n\n{punchline}"));
            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Error fetching joke: {ex.Message}"));
            }
        }

        [SlashCommand("quote", "Get a random quote.")]
        public async Task QuoteCommand(InteractionContext ctx)
        {
            string apiUrl = "https://zenquotes.io/api/random";
            using var http = new HttpClient();
            try
            {
                var response = await http.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Could not fetch a quote at the moment."));
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var quoteObj = root[0];
                    var content = quoteObj.GetProperty("q").GetString();
                    var author = quoteObj.GetProperty("a").GetString();

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"\"{content}\"\n- {author}"));
                }
                else
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No quote found in the response."));
                }
            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Error fetching quote: {ex.Message}"));
            }
        }

        [SlashCommand("purge", "Purges messages from the channel.")]
        public async Task PurgeCommand(InteractionContext ctx, [Option("count", "Number of messages to purge")] long count)
        {
            if (count <= 0 || count > 1000)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Please specify a number between 1 and 1000."));
                return;
            }

            var messages = await ctx.Channel.GetMessagesAsync((int)count);
            await ctx.Channel.DeleteMessagesAsync(messages);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{messages.Count} messages purged."));
        }

        // Removed duplicate RoleInfoCommand to resolve method conflict.

        [SlashCommand("botping", "Check the bot's latency.")]
        public async Task BotPingCommand(InteractionContext ctx)
        {
            var latency = ctx.Client.Ping;
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Bot latency: {latency} ms"));
        }

        [SlashCommand("avatarhistory", "Get the avatar history of a user.")]
        public async Task AvatarHistoryCommand(InteractionContext ctx, [Option("user", "The user to get the avatar history of")] DiscordUser user)
        {
            if (user == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Please specify a valid user."));
                return;
            }

            // For simplicity, we will just return the current avatar URL
            var embed = new DiscordEmbedBuilder
            {
                Title = $"{user.Username}'s Avatar History",
                Description = $"Current Avatar: [Click here]({user.AvatarUrl})",
                Color = DiscordColor.Purple
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("purgeuser", "Purge messages from a specific user in the channel.")]
        public async Task PurgeUserCommand(InteractionContext ctx, [Option("user", "The user whose messages to purge")] DiscordUser user, [Option("count", "Number of messages to purge")] long count = 1000)
        {
            if (count <= 0 || count > 1000)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Please specify a number between 1 and 1000."));
                return;
            }

            var messages = await ctx.Channel.GetMessagesAsync((int)count);
            var userMessages = messages.Where(m => m.Author.Id == user.Id).ToList();

            if (userMessages.Count == 0)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"No messages found from {user.Username} in the last {count} messages."));
                return;
            }

            await ctx.Channel.DeleteMessagesAsync(userMessages);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{userMessages.Count} messages from {user.Username} purged."));
        }

        [SlashCommand("purgerole", "Purge messages from a specific role in the channel.")]
        public async Task PurgeRoleCommand(InteractionContext ctx, [Option("role", "The role whose messages to purge")] DiscordRole role, [Option("count", "Number of messages to purge")] long count = 1000)
        {
            if (count <= 0 || count > 1000)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Please specify a number between 1 and 1000."));
                return;
            }

            var messages = await ctx.Channel.GetMessagesAsync((int)count);
            var roleMessages = messages.Where(m => m.Author is DiscordMember member && member.Roles.Any(r => r.Id == role.Id)).ToList();

            if (roleMessages.Count == 0)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"No messages found from users with the role {role.Name} in the last {count} messages."));
                return;
            }

            await ctx.Channel.DeleteMessagesAsync(roleMessages);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{roleMessages.Count} messages from users with the role {role.Name} purged."));
        }

        [SlashCommand("serverbooster", "Get information about the server booster.")]
        public async Task ServerBoosterCommand(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Server Booster Information",
                Description = "Boosting the server helps us grow and provides you with special perks! Join us in making this server better.",
                Color = DiscordColor.Gold
            };

            embed.AddField("Boost Benefits", "Boosting the server grants you access to exclusive channels, emojis, and more!");
            embed.AddField("How to Boost", "Click on the 'Boost' button at the top of the server to support us!");

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }


        [SlashCommand("boostinfo", "Check if a user has boosted the server and for how long.")]
        public async Task BoostInfoCommand(
            InteractionContext ctx,
            [Option("user", "The user to check (leave blank for yourself)")] DiscordUser user = null)
        {
            user ??= ctx.User;

            DiscordMember member;
            try
            {
                member = await ctx.Guild.GetMemberAsync(user.Id);
            }
            catch
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("User is not a member of this server."));
                return;
            }

            if (member.PremiumSince.HasValue)
            {
                var boostStart = member.PremiumSince.Value.UtcDateTime;
                var now = DateTime.UtcNow;
                var duration = now - boostStart;

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{member.DisplayName} is a Server Booster!",
                    Description = $"Boosting since: {boostStart:f} UTC\n" +
                                  $"Duration: {duration.Days} days, {duration.Hours} hours, {duration.Minutes} minutes",
                    Color = DiscordColor.Magenta
                };

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"{member.DisplayName} is not boosting this server."));
            }
        }

        [SlashCommand("roleinfo", "Get information about a role in the server.")]
        public async Task RoleInfoCommand(InteractionContext ctx, [Option("role", "The role to get information about")] DiscordRole role)
        {
            if (role == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Please specify a valid role."));
                return;
            }

            // Count members with this role
            int memberCount = ctx.Guild.Members.Values.Count(m => m.Roles.Any(r => r.Id == role.Id));

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{role.Name} Role Information",
                Color = role.Color,
                Description = $"**ID:** {role.Id}\n" +
                              $"**Color:** {role.Color}\n" +
                              $"**Position:** {role.Position}\n" +
                              $"**Mentionable:** {(role.IsMentionable ? "Yes" : "No")}\n" +
                              $"**Hoisted:** {(role.IsHoisted ? "Yes" : "No")}\n" +
                              $"**Managed:** {(role.IsManaged ? "Yes" : "No")}\n" +
                              $"**Permissions:** {role.Permissions}\n" +
                              $"**Member Count:** {memberCount}"
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("activity", "Show the most active users or channels in the server.")]
        public async Task ActivityCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var userMessageCounts = new Dictionary<ulong, int>();
            var channelMessageCounts = new Dictionary<ulong, int>();

            //Limit to last 1000 messages per channel for performance
            int messagesToFetch = 1000;

            foreach (var channel in ctx.Guild.Channels.Values.Where(c => c.Type == ChannelType.Text))
            {
                try
                {
                    var messages = await channel.GetMessagesAsync(messagesToFetch);
                    channelMessageCounts[channel.Id] = messages.Count;

                    foreach (var msg in messages)
                    {
                        if (msg.Author.IsBot) continue; // Ignore bots
                        if (!userMessageCounts.ContainsKey(msg.Author.Id))
                            userMessageCounts[msg.Author.Id] = 0;
                        userMessageCounts[msg.Author.Id]++;

                    }
                }
                catch
                {
                    // Ignore channels where bot can't read
                }
            }

            // Top 5 users
            var topUsers = userMessageCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(5)
            .Select(kvp =>
            {
                var member = ctx.Guild.Members.TryGetValue(kvp.Key, out var m) ? m.DisplayName : $"<@{kvp.Key}>";
                return $"#{member}: {kvp.Value} messages";
            });

            // Top 5 Channels
            var topChannels = channelMessageCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(5)
            .Select(kvp =>
            {
                var channel = ctx.Guild.GetChannel(kvp.Key);
                return $"{channel.Name}: {kvp.Value} messages";
            });

            var embed = new DiscordEmbedBuilder
            {
                Title = "Server Activity Stats (last 1000 messages per channel)",
                Color = DiscordColor.Blurple
            }
            .AddField("Top Active Users", string.Join("\n", topUsers), true)
            .AddField("Top Active Channels", string.Join("\n", topChannels), true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
    }
}