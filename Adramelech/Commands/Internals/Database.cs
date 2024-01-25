using Adramelech.Extensions;
using Adramelech.Models;
using Adramelech.Services;
using Adramelech.Utilities;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

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

            var updateToken = await dbService.UpsertConfigAsync(new ConfigModel
            {
                Key = "ApiToken",
                Value = hash
            });
            var updateTokenKey = await dbService.UpsertConfigAsync(new ConfigModel
            {
                Key = "ApiTokenKey",
                Value = key
            });

            if (updateToken.IsFailure || updateTokenKey.IsFailure ||
                // if the values are not the same as the ones we want to set, the update failed
                updateToken.Value?.Value != hash || updateTokenKey.Value?.Value != key)
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

            var config = new ConfigModel
            {
                Key = "FilesChannelId",
                Value = channel.Id.ToString()
            };

            var result = await dbService.UpsertConfigAsync(config);
            // If the values is not the same as the one we want to set, the update failed
            if (result.IsFailure || result.Value?.Value != config.Value)
            {
                await Context.SendError("Failed to update files channel.", true);
                return;
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
}