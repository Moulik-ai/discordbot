using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace discordbot.commands
{
    public class MusicCommands : ApplicationCommandModule
    {
        // GuildId -> Queue
        private static readonly Dictionary<ulong, Queue<LavalinkTrack>> _queues = new();
        // GuildId -> IsPlaying
        private static readonly Dictionary<ulong, bool> _isPlaying = new();

        [SlashCommand("join", "Joins your current voice channel.")]
        public async Task Join(InteractionContext ctx)
        {
            var channel = ctx.Member?.VoiceState?.Channel;
            if (channel == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You need to be in a voice channel!"));
                return;
            }

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Lavalink is not connected."));
                return;
            }

            var node = lava.ConnectedNodes.Values.First();
            await node.ConnectAsync(channel);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Joined {channel.Name}!"));
        }

        [SlashCommand("play", "Plays music from a search term or URL.")]
        public async Task Play(InteractionContext ctx, [Option("query", "Search term or URL")] string search)
        {
            var channel = ctx.Member?.VoiceState?.Channel;
            if (channel == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You need to be in a voice channel!"));
                return;
            }

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Lavalink is not connected."));
                return;
            }

            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);

            if (conn == null)
            {
                await node.ConnectAsync(channel);
                conn = node.GetGuildConnection(ctx.Guild);
            }

            var loadResult = await node.Rest.GetTracksAsync(search);
            var track = loadResult.Tracks.FirstOrDefault();
            if (track == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No results found."));
                return;
            }

            // Add to queue
            if (!_queues.ContainsKey(ctx.Guild.Id))
                _queues[ctx.Guild.Id] = new Queue<LavalinkTrack>();
            _queues[ctx.Guild.Id].Enqueue(track);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Queued: {track.Title}"));

            // If not already playing, start playback
            if (!_isPlaying.ContainsKey(ctx.Guild.Id) || !_isPlaying[ctx.Guild.Id])
            {
                await PlayNextAsync(ctx, conn, ctx.Guild.Id);
            }

            // Register event handler if not already registered
            conn.PlaybackFinished -= OnPlaybackFinished;
            conn.PlaybackFinished += OnPlaybackFinished;
        }

        [SlashCommand("queue", "Shows the current music queue.")]
        public async Task Queue(InteractionContext ctx)
        {
            if (!_queues.TryGetValue(ctx.Guild.Id, out var queue) || queue.Count == 0)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The queue is empty."));
                return;
            }

            var list = queue.ToList();
            string queueList = string.Join("\n", list.Select((t, i) => $"{i + 1}. {t.Title}"));
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Current queue:\n{queueList}"));
        }

        [SlashCommand("skip", "Skips the current track.")]
        public async Task Skip(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Lavalink is not connected."));
                return;
            }

            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);
            if (conn == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("I'm not in a voice channel."));
                return;
            }

            await conn.StopAsync();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Skipped the current track."));
        }

        [SlashCommand("leave", "Leaves the voice channel.")]
        public async Task Leave(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Lavalink is not connected."));
                return;
            }

            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);
            if (conn != null)
            {
                await conn.DisconnectAsync();
                _isPlaying[ctx.Guild.Id] = false;
                if (_queues.ContainsKey(ctx.Guild.Id))
                    _queues[ctx.Guild.Id].Clear();
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Left the voice channel."));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("I'm not in a voice channel."));
            }
        }

        // Helper: Play next track in queue
        private async Task PlayNextAsync(InteractionContext ctx, LavalinkGuildConnection conn, ulong guildId)
        {
            if (!_queues.TryGetValue(guildId, out var queue) || queue.Count == 0)
            {
                _isPlaying[guildId] = false;
                return;
            }

            var nextTrack = queue.Dequeue();
            _isPlaying[guildId] = true;
            await conn.PlayAsync(nextTrack);

            // Optionally, send a message to the channel
            var channel = ctx.Channel;
            await channel.SendMessageAsync($"Now playing: {nextTrack.Title}");
        }

        // Event handler for when a track finishes
        private async Task OnPlaybackFinished(LavalinkGuildConnection conn, DSharpPlus.Lavalink.EventArgs.TrackFinishEventArgs e)
        {
            var guildId = conn.Guild.Id;
            if (!_queues.TryGetValue(guildId, out var queue) || queue.Count == 0)
            {
                _isPlaying[guildId] = false;
                return;
            }

            // Get a context for the guild to send messages (find a text channel)
            var channel = conn.Guild.Channels.Values.FirstOrDefault(c => c.Type == ChannelType.Text);
            if (channel == null)
                return;

            // Play next track
            var nextTrack = queue.Dequeue();
            _isPlaying[guildId] = true;
            await conn.PlayAsync(nextTrack);
            await channel.SendMessageAsync($"Now playing: {nextTrack.Title}");
        }

        [SlashCommand("stop", "Stops the music and clears the queue.")]
        public async Task Stop(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Lavalink is not connected."));
                return;
            }

            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);
            if (conn == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("I'm not in a voice channel."));
                return;
            }

            await conn.StopAsync();
            _isPlaying[ctx.Guild.Id] = false;
            if (_queues.ContainsKey(ctx.Guild.Id))
                _queues[ctx.Guild.Id].Clear();

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Stopped the music and cleared the queue."));
        }

        [SlashCommand("clear_queue", "Clears the music queue.")]
        public async Task clearQueue(InteractionContext ctx)
        {
            if (_queues.ContainsKey(ctx.Guild.Id))
            {
                _queues[ctx.Guild.Id].Clear();
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Cleared the music queue."));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The queue is already empty."));
            }
        }
    }
}