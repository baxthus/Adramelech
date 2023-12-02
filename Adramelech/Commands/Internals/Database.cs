using System.Text;
using Adramelech.Configuration;
using Adramelech.Database;
using Adramelech.Extensions;
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
    [Group("music", "Music database commands")]
    public class Music : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
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

            try
            {
                await DatabaseManager.Music.InsertOneAsync(new MusicSchema
                {
                    Id = ObjectId.GenerateNewId(),
                    Title = title,
                    Album = album,
                    Artist = artist,
                    Url = url,
                    Favorite = favorite
                });
            }
            catch
            {
                await Context.ErrorResponse("Failed to add song to database.", true);
                return;
            }

            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithColor(BotConfig.EmbedColor)
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

            List<MusicSchema> songs;
            try
            {
                songs = await DatabaseManager.Music.Find(_ => true).ToListAsync();
            }
            catch
            {
                await Context.ErrorResponse("Failed to get songs from database.", true);
                return;
            }

            if (!songs.Any())
            {
                await Context.ErrorResponse("No songs found in database.", true);
                return;
            }

            StringBuilder sb = new();

            songs.ForEach(x =>
                sb.AppendLine($"ID: {x.Id}\n" +
                              $"Title: {x.Title}\n" +
                              $"Album: {x.Album}\n" +
                              $"Artist: {x.Artist}\n" +
                              $"URL: {x.Url}\n" +
                              $"Favorite: {x.Favorite}\n"));

            await FollowupWithFileAsync(
                embed: new EmbedBuilder()
                    .WithColor(BotConfig.EmbedColor)
                    .WithTitle("Songs in database")
                    .Build(),
                ephemeral: true,
                fileName: "songs.txt",
                fileStream: new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString())));
        }
    }
}

public class DatabaseComponents : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    [ComponentInteraction("removeSongYes")]
    public async Task RemoveSongYesAsync()
    {
        // Parse the song id between the backticks
        var id = Context.Interaction.Message.Content.Split('`')[1];

        bool deleted;
        try
        {
            var result = await DatabaseManager.Music.DeleteOneAsync(x => x.Id == ObjectId.Parse(id));
            deleted = result.DeletedCount > 0;
        }
        catch
        {
            await Context.ErrorResponse("Failed to remove song from database.", InteractionOrigin.Component);
            return;
        }

        if (!deleted)
        {
            await Context.ErrorResponse("Song not found in database.", InteractionOrigin.Component);
            return;
        }

        await Context.Interaction.UpdateAsync(p =>
        {
            p.Embed = new EmbedBuilder()
                .WithColor(BotConfig.EmbedColor)
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