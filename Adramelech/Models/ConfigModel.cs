using System.Diagnostics.CodeAnalysis;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Adramelech.Models;

[Table("config")]
[SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
public class ConfigModel : BaseModel
{
    [PrimaryKey("key")] public string Key { get; init; } = null!;
    [Column("value")] public string Value { get; init; } = null!;
}