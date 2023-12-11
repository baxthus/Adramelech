using System.Net;
using Adramelech.Http.Attributes;
using Adramelech.Http.Common;
using Adramelech.Http.Extensions;
using Adramelech.Http.Utilities;
using Adramelech.Utilities;

namespace Adramelech.Http.Endpoints.Files;

[Endpoint("/files/download")]
[NeedsToken]
public class DownloadEndpoint : EndpointBase
{
    protected override async Task HandleAsync()
    {
        var ids = Request.QueryString["ids"]?.Split(',');
        if (ids is null or { Length: 0 })
        {
            await Context.RespondAsync("Missing ids parameter", HttpStatusCode.BadRequest);
            return;
        }

        var channel = await FilesEndpointUtils.GetChannel(BotClient);
        if (channel is null)
        {
            await Context.RespondAsync("Channel not found", HttpStatusCode.InternalServerError);
            return;
        }

        var image = new List<byte[]>();

        foreach (var id in ids)
        {
            if (!ulong.TryParse(id, out var messageId))
            {
                await Context.RespondAsync($"Invalid message id: {id}", HttpStatusCode.BadRequest);
                return;
            }

            var message = await channel.GetMessageAsync(messageId);
            if (message is null)
            {
                await Context.RespondAsync($"Message not found: {id}", HttpStatusCode.NotFound);
                return;
            }

            var attachment = message.Attachments.FirstOrDefault();
            if (attachment is null)
            {
                await Context.RespondAsync($"Message has no attachments: {id}", HttpStatusCode.BadGateway);
                return;
            }

            var response = await attachment.Url.Request<byte[]>();
            if (response is null)
            {
                await Context.RespondAsync($"Failed to download image: {id}", HttpStatusCode.BadGateway);
                return;
            }

            image.Add(response);
        }

        await Context.RespondAsync(CombineBytes(image),
            contentType: Request.QueryString["contentType"] ?? "application/octet-stream");
    }

    private static byte[] CombineBytes(List<byte[]> bytes)
    {
        var result = new byte[bytes.Sum(x => x.Length)];
        var offset = 0;
        foreach (var array in bytes)
        {
            Buffer.BlockCopy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }

        return result;
    }
}