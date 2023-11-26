using MongoDB.Bson;

namespace Adramelech.Database;

public struct MusicSchema
{
    public ObjectId Id { get; set; }
    public string Title { get; set; }
    public string Album { get; set; }
    public string Artist { get; set; }
    public string Url { get; set; }
    public bool Favorite { get; set; }
}