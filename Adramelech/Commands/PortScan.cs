using Adramelech.Configuration;
using Adramelech.Data;
using Adramelech.Extensions;
using Adramelech.Tools;
using Adramelech.Utilities;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

[Group("port-scan", "Scan ports on a host")]
public class PortScan : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    private static Embed _defaultResponse(string host) => new EmbedBuilder()
        .WithColor(Color.LighterGrey)
        .WithTitle($"Port scan on `{host}` started")
        .WithDescription("You will receive a DM when the scan is complete.\n" +
                         "Please be sure to enable DMs from server members.")
        .Build();

    [SlashCommand("in-range", "Scan ports in a range")]
    public async Task InRangeAsync(
        [Summary("host", "The host to scan")] string host,
        [Summary("start", "The port to start scanning at")] [MinValue(1)] [MaxValue(65535)]
        int start,
        [Summary("end", "The port to end scanning at")] [MinValue(1)] [MaxValue(65535)]
        int end)
    {
        var ports = Enumerable.Range(start, end - start + 1).ToList();

        RunPortScanAsync(host, ports);

        await RespondAsync(embed: _defaultResponse(host), ephemeral: true);
    }

    [SlashCommand("in-list", "Scan ports in a list")]
    public async Task InListAsync(
        [Summary("host", "The host to scan")] string host,
        [Summary("ports", "The ports to scan separated by spaces")]
        string ports)
    {
        List<int> portList;
        try
        {
            portList = ports.Split(' ').Select(int.Parse).ToList();
        }
        catch
        {
            await Context.SendError("Invalid format");
            return;
        }

        RunPortScanAsync(host, portList);

        await RespondAsync(embed: _defaultResponse(host), ephemeral: true);
    }

    [SlashCommand("common", "Scan common ports")]
    public async Task CommonAsync(
        [Summary("host", "The host to scan")] string host)
    {
        RunPortScanAsync(host, CommonOpenPorts.Ports);

        await RespondAsync(embed: _defaultResponse(host), ephemeral: true);
    }

    private void RunPortScanAsync(string host, List<int> ports) =>
        // Don't await the task, so the response can be sent immediately
        Task.Run(async () =>
        {
            // Cancel the task after 15 minutes
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(15));

            var result =
                await ExceptionUtils.TryAsync(() => new PortScanner(host, ports, Context.User, cts.Token).ScanAsync());
            if (result.IsFailure)
            {
                await Context.SendError(result.Exception!.Message, toDm: true);
                return;
            }

            if (result.Value!.Count == 0)
            {
                await Context.SendError("No open ports found", toDm: true);
                return;
            }

            await Context.User.SendMessageAsync(embed: new EmbedBuilder()
                .WithColor(BotConfig.EmbedColor)
                .WithTitle($"Port scan on `{host}` complete")
                .WithDescription("The port scan that you requested is complete, you can see the results below.\n" +
                                 "Be aware that some ports may be closed but still show up as open.\n" +
                                 "Also be aware rate limiting may have occurred, so some ports may not have been scanned.")
                .AddField(":unlock: Open ports", $"```{string.Join(", ", result.Value)}```")
                .Build());
        });
}