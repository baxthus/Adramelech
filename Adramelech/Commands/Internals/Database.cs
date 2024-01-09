using System.Text;
using Adramelech.Database;
using Adramelech.Extensions;
using Adramelech.Services;
using Adramelech.Utilities;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Adramelech.Commands.Internals;

[Group("database", "Database commands")]
[RequireOwner]
public class Database : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [Group("config", "Configuration database commands")]
    public class Config(ConfigService configService, DatabaseService dbService)
        : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
    {
        [SlashCommand("regen-api-token", "Regenerate the API token")]
        public async Task RegenApiTokenAsync()
        {
            await DeferAsync(true);

            // Yeah, I know this is not really random, but it's good enough for me
            var apiToken = Guid.NewGuid().ToString("N");
            var key = EncryptUtils.DeriveKey(apiToken);
            var hash = await EncryptUtils.Encrypt(apiToken, key);

            var updateToken = await dbService.UpdateConfigAsync(new ConfigSchema
            {
                Key = "ApiToken",
                Value = hash
            });
            var updateTokenKey = await dbService.UpdateConfigAsync(new ConfigSchema
            {
                Key = "ApiTokenKey",
                Value = key
            });

            if (updateToken.IsFailure || updateTokenKey.IsFailure ||
                updateToken.Value?.ModifiedCount == 0 || updateTokenKey.Value?.ModifiedCount == 0)
            {
                await Context.SendError("Failed to regenerate API token.", true);
                return;
            }


            await configService.ReloadAsync();

            var button = new ComponentBuilder()
                .WithButton("Setup cookie", style: ButtonStyle.Link,
                    url: $"{configService.BaseUrl}/auth/setup-cookie?token={apiToken}")
                .Build();

            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithColor(ConfigService.EmbedColor)
                    .WithTitle("API token regenerated")
                    .WithDescription($"**New API token:** `{apiToken}`")
                    .Build(),
                components: button,
                ephemeral: true);
        }

        [SlashCommand("files-channel", "Get or set the files channel")]
        public async Task FilesChannelAsync(
            [Summary("channel", "The channel to set as files channel")]
            SocketTextChannel? channel = null)
        {
            await DeferAsync(true);

            if (channel is null)
            {
                var channelId = configService.FilesChannel;
                if (!channelId.HasValue)
                {
                    await Context.SendError("Files channel not set.", true);
                    return;
                }

                var filesChannel = Context.Guild.GetChannel(channelId.Value);

                await FollowupAsync(
                    embed: new EmbedBuilder()
                        .WithColor(ConfigService.EmbedColor)
                        .WithTitle("Files channel")
                        .WithDescription($"**ID:** `{filesChannel.Id}`\n" +
                                         $"**Name:** {filesChannel.Name}\n" +
                                         $"**Guild:** {filesChannel.Guild.Name} (`{filesChannel.Guild.Id}`)")
                        .Build(),
                    ephemeral: true);
                return;
            }

            var exist = await dbService.ConfigExistsAsync("FilesChannelId");
            if (exist.IsFailure)
            {
                await Context.SendError("Failed to get files channel.", true);
                return;
            }

            if (exist.Value)
            {
                if (await dbService.UpdateConfigAsync(new ConfigSchema
                    {
                        Key = "FilesChannelId",
                        Value = channel.Id.ToString()
                    }) is { IsFailure: true, Value.ModifiedCount: 0 })
                {
                    await Context.SendError("Failed to update files channel.", true);
                    return;
                }
            }
            else
            {
                if (await dbService.InsertConfigAsync(new ConfigSchema
                    {
                        Id = ObjectId.GenerateNewId(),
                        Key = "FilesChannelId",
                        Value = channel.Id.ToString()
                    }) is { IsFailure: true })
                {
                    await Context.SendError("Failed to set files channel.", true);
                    return;
                }
            }

            await configService.ReloadAsync();

            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithColor(ConfigService.EmbedColor)
                    .WithTitle("Files channel set")
                    .WithDescription($"**ID:** `{channel.Id}`\n" +
                                     $"**Name:** {channel.Name}")
                    .Build(),
                ephemeral: true);
        }
    }

    [Group("music", "Music database commands")]
    public class Music(DatabaseService dbService) : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
    {
        [SlashCommand("add", "Add a song to the database")]
        public async Task AddAsync([Summary("title", "The title of the song")] string title,
            [Summary("album", "The album of the song")]
            string album,
            [Summary("artist", "The artist of the song")]
            string artist,
            [Summary("url", "The URL of the song")]
            string url,
            [Summary("favorite", "Whether the song is a favorite")]
            bool favorite)
        {
            await DeferAsync(true);

            var result = await ExceptionUtils.TryAsync(() => dbService.Music.InsertOneAsync(new MusicSchema
            {
                Id = ObjectId.GenerateNewId(),
                Title = title,
                Album = album,
                Artist = artist,
                Url = url,
                Favorite = favorite
            }));

            if (result.IsFailure)
            {
                await Context.SendError("Failed to add song to database.", true);
                return;
            }

            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithColor(ConfigService.EmbedColor)
                    .WithTitle("Song added to database")
                    .WithDescription($"**Title:** {title}\n" +
                                     $"**Album:** {album}\n" +
                                     $"**Artist:** {artist}\n" +
                                     $"**URL:** {url}\n" +
                                     $"**Favorite:** {favorite}")
                    .Build(),
                ephemeral: true);
        }

        [SlashCommand("remove", "Remove a song from the database")]
        public async Task RemoveAsync([Summary("id", "The ID of the song")] string id) =>
            await RespondAsync(
                text: $"Are you sure you want to remove song with id `{id}` from the database?",
                components: new ComponentBuilder()
                    .WithButton("Yes", "removeSongYes", ButtonStyle.Success)
                    .WithButton("No", "removeSongNo", ButtonStyle.Danger)
                    .Build(),
                ephemeral: true);

        [SlashCommand("list", "List all songs in the database")]
        public async Task ListAsync()
        {
            await DeferAsync(true);

            var result = await ExceptionUtils.TryAsync(() => dbService.Music.Find(_ => true).ToListAsync());
            if (result.IsFailure)
            {
                await Context.SendError("Failed to get songs from database.", true);
                return;
            }

            var songs = result.Value;

            if (songs?.Count == 0)
            {
                await Context.SendError("No songs found in database.", true);
                return;
            }

            StringBuilder sb = new();

            songs?.ForEach(x =>
                sb.AppendLine($"ID: {x.Id}\n" +
                              $"Title: {x.Title}\n" +
                              $"Album: {x.Album}\n" +
                              $"Artist: {x.Artist}\n" +
                              $"URL: {x.Url}\n" +
                              $"Favorite: {x.Favorite}\n"));

            await FollowupWithFileAsync(
                embed: new EmbedBuilder()
                    .WithColor(ConfigService.EmbedColor)
                    .WithTitle("Songs in database")
                    .Build(),
                ephemeral: true,
                fileName: "songs.txt",
                fileStream: new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString())));
        }
    }
}

public class DatabaseComponents(DatabaseService dbService)
    : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    [ComponentInteraction("removeSongYes")]
    public async Task RemoveSongYesAsync()
    {
        // Parse the song id between the backticks
        var id = Context.Interaction.Message.Content.Split('`')[1];

        var deleted =
            await ExceptionUtils.TryAsync(() => dbService.Music.DeleteOneAsync(x => x.Id == ObjectId.Parse(id)));
        if (deleted.IsFailure)
        {
            await Context.SendError("Failed to remove song from database.");
            return;
        }

        if (deleted.Value?.DeletedCount == 0)
        {
            await Context.SendError("Song not found in database.");
            return;
        }

        await Context.Interaction.UpdateAsync(p =>
        {
            p.Embed = new EmbedBuilder()
                .WithColor(ConfigService.EmbedColor)
                .WithTitle("Song removed from database")
                .Build();
            // Remove the buttons and content
            p.Content = "";
            p.Components = new ComponentBuilder().Build();
        });
    }

    [ComponentInteraction("removeSongNo")]
    public async Task RemoveSongNoAsync()
    {
        await Context.Interaction.UpdateAsync(p =>
        {
            p.Embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("Operation cancelled")
                .Build();
            // Remove the buttons and content
            p.Content = "";
            p.Components = new ComponentBuilder().Build();
        });
    }
}