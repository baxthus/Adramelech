using MongoDB.Bson;

namespace Adramelech.Models;

public struct ConfigModel
{
    public ObjectId Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}