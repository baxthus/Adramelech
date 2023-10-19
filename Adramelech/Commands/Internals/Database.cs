using System.Text;
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
            [Summary("album", "The album of the song")] string album,
            [Summary("artist", "The artist of the song")] string artist,
            [Summary("url", "The URL of the song")] string url,
            [Summary("favorite", "Whether the song is a favorite")] bool favorite)
        {
            await Context.Interaction.DeferAsync(true);
        
            try
            {
                await global::Adramelech.Database.Music.InsertOneAsync(new global::Adramelech.Database.MusicSchema
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
                await Context.ErrorResponse("Failed to add song to database.", isDeferred: true);
                return;
            }
        
            var embed = new EmbedBuilder()
                .WithColor(Config.Bot.EmbedColor)
                .WithTitle("Song added to database")
                .WithDescription(
                    $"**Title:** {title}\n**Album:** {album}\n**Artist:** {artist}\n**URL:** {url}\n**Favorite:** {favorite}")
                .Build();
        
            await FollowupAsync(embed: embed, ephemeral: true);
        }
        
        [SlashCommand("remove", "Remove a song from the database")]
        public async Task RemoveAsync([Summary("title", "The title of the song")] string title,
            [Summary("album", "The album of the song")] string album,
            [Summary("artist", "The artist of the song")] string artist)
        {
            await Context.Interaction.DeferAsync(true);
        
            try
            {
                await global::Adramelech.Database.Music.DeleteOneAsync(x =>
                    x.Title == title && x.Album == album && x.Artist == artist);
            }
            catch
            {
                await Context.ErrorResponse("Failed to remove song from database.", isDeferred: true);
                return;
            }
        
            var embed = new EmbedBuilder()
                .WithColor(Config.Bot.EmbedColor)
                .WithTitle("Song removed from database")
                .WithDescription($"**Title:** {title}\n**Album:** {album}\n**Artist:** {artist}")
                .Build();
        
            await FollowupAsync(embed: embed, ephemeral: true);
        }
        
        [SlashCommand("list", "List all songs in the database")]
        public async Task ListAsync()
        {
            await Context.Interaction.DeferAsync(true);

            List<global::Adramelech.Database.MusicSchema> songs;
            try
            {
                songs = await global::Adramelech.Database.Music.Find(_ => true).ToListAsync();
            }
            catch
            {
                await Context.ErrorResponse("Failed to get songs from database.", isDeferred: true);
                return;
            }
        
            StringBuilder sb = new();
            foreach (var song in songs)
            {
                sb.AppendLine($"**Title:** {song.Title}\n" +
                              $"**Album:** {song.Album}\n" +
                              $"**Artist:** {song.Artist}\n" +
                              $"**URL:** {song.Url}\n" +
                              $"**Favorite:** {song.Favorite}\n\n");
            }
        
            var embed = new EmbedBuilder()
                .WithColor(Config.Bot.EmbedColor)
                .WithTitle("Songs in database")
                .Build();
            
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        
            await FollowupWithFileAsync(embed: embed, ephemeral: true, fileName: "songs.txt", fileStream: stream);
        }
    }
}