namespace Adramelech.Http.Schemas;

public class FileChunkSchema
{
    public ulong MessageId { get; set; }
    public int CurrentChunk { get; set; }
}