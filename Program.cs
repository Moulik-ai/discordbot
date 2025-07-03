using discordbot.config;
using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;
using discordbot.commands;
using DSharpPlus.Entities;
using System.Collections.Generic;

internal class Program
{
    private static DiscordClient? Client { get; set; }

    // Store sniped messages per channel for the /snipe command
    public static Dictionary<ulong, (string Content, DiscordUser Author)> SnipedMessages = new();

    static async Task Main(string[] args)
    {
        // Read config
        var jsonReader = new JSONreader();
        await jsonReader.ReadJSON();

        // Configure Discord client
        var discordConfig = new DiscordConfiguration
        {
            Token = jsonReader.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.Guilds | DiscordIntents.GuildMessages | DiscordIntents.MessageContents | DiscordIntents.GuildMembers,
        };

        Client = new DiscordClient(discordConfig);

        // Register the welcome event handler
        Client.GuildMemberAdded += async (client, e) =>
        {
            ulong welcomeChannelId = 1389203118704689152; // Use your desired channel ID
            var channel = await client.GetChannelAsync(welcomeChannelId);
            if (channel != null)
            {
                await channel.SendMessageAsync($"Welcome to the server, {e.Member.Mention}!");
            }
        };

        // Register MessageDeleted event for /snipe
        Client.MessageDeleted += async (client, e) =>
    {
    // Debug logging
    Console.WriteLine($"[DEBUG] MessageDeleted event fired in channel {e.Channel.Id}");
    if (e.Message != null)
    {
        Console.WriteLine($"[DEBUG] Message content: '{e.Message.Content}' by {e.Message.Author?.Username}");
    }
    else
    {
        Console.WriteLine("[DEBUG] e.Message is null");
    }

    if (e.Message != null && !string.IsNullOrWhiteSpace(e.Message.Content) && e.Message.Author != null)
    {
        SnipedMessages[e.Channel.Id] = (e.Message.Content, e.Message.Author);
    }
    };

        // Register slash commands
        var slash = Client.UseSlashCommands();

        // Register your command modules here
        slash.RegisterCommands<TestCommands>();
        slash.RegisterCommands<MusicCommands>();

        // Optional: Log slash command errors for debugging
        slash.SlashCommandErrored += static async (s, e) =>
        {
            if (e.Context != null)
            {
                try
                {
                    await e.Context.CreateResponseAsync($"Error: {e.Exception.GetType().Name} - {e.Exception.Message}", true);
                }
                catch
                {
                    // If already responded, send a follow-up instead
                    await e.Context.FollowUpAsync(new DiscordFollowupMessageBuilder()
                        .WithContent($"Error: {e.Exception.GetType().Name} - {e.Exception.Message}")
                        .AsEphemeral(true));
                }
            }
        };

        // Lavalink setup
        var endpoint = new ConnectionEndpoint
        {
            Hostname = "lava-v3.ajieblogs.eu.org", // Lavalink server address
            Port = 443,             // Lavalink default port
            Secured = true // Use true for secure connections (SSL/TLS)
        };

        var lavalinkConfig = new LavalinkConfiguration
        {
            Password = "https://dsc.gg/ajidevserver", // Default Lavalink password
            RestEndpoint = endpoint,
            SocketEndpoint = endpoint
        };


        var lavalink = Client.UseLavalink();

        // Connect Discord client first!
        await Client.ConnectAsync();

        // Then connect Lavalink
        await lavalink.ConnectAsync(lavalinkConfig);

        await Task.Delay(-1);
    }
}