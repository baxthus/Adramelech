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
            await Context.Interaction.DeferAsync(true);

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
        public async Task RemoveAsync([Summary("id", "The ID of the song")] string id)
        {
            await Context.Interaction.DeferAsync(true);

            try
            {
                await DatabaseManager.Music.DeleteOneAsync(x => x.Id == ObjectId.Parse(id));
            }
            catch
            {
                await Context.ErrorResponse("Failed to remove song from database.", true);
                return;
            }

            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithColor(BotConfig.EmbedColor)
                    .WithTitle("Song removed from database")
                    .Build(),
                ephemeral: true);
        }

        [SlashCommand("list", "List all songs in the database")]
        public async Task ListAsync()
        {
            await Context.Interaction.DeferAsync(true);

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

            if (songs.Count == 0) await Context.ErrorResponse("No songs found in database.", true);

            StringBuilder sb = new();
            foreach (var song in songs)
                sb.AppendLine($"ID: {song.Id}\n" +
                              $"Title: {song.Title}\n" +
                              $"Album: {song.Album}\n" +
                              $"Artist: {song.Artist}\n" +
                              $"URL: {song.Url}\n" +
                              $"Favorite: {song.Favorite}\n");

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