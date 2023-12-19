using MongoDB.Bson;

namespace Adramelech.Http.Schemas;

public struct FileSchema
{
    public ObjectId Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? FileName { get; set; }
    public string ContentType { get; set; }
    public int TotalChunks { get; set; }
    public List<FileChunkSchema> Chunks { get; set; }
}