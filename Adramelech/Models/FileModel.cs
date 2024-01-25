using System.Diagnostics.CodeAnalysis;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Adramelech.Models;

[Table("files")]
[SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
public class FileModel : BaseModel
{
    [PrimaryKey("id")] public Guid Id { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; }
    [Column("available")] public bool Available { get; set; }
    [Column("file_name")] public string? FileName { get; set; }
    [Column("content_type")] public string ContentType { get; set; } = null!;
    [Column("total_chunks")] public int TotalChunks { get; set; }
    [Column("chunks")] public List<FileChunkModel> Chunks { get; set; } = null!;
}

public record FileChunkModel()
{
    public ulong MessageId { get; set; }
    public int CurrentChunk { get; set; }
};