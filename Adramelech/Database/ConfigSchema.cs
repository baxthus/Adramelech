using MongoDB.Bson;

namespace Adramelech.Database;

public struct ConfigSchema
{
    public ObjectId Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}