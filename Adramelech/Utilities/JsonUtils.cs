using Newtonsoft.Json;

namespace Adramelech.Utilities;

/// <summary>
/// Utility class for JSON
/// </summary>
public static class JsonUtils
{
    /// <summary>
    /// Serialize an <see cref="object"/> to an JSON <see cref="string"/>
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to serialize (can be implicit)</param>
    /// <returns>The serialized <see cref="string"/> of the <see cref="object"/></returns>
    /// <seealso cref="FromJson{T}"/>
    public static string ToJson(this object obj) => JsonConvert.SerializeObject(obj);

    /// <summary>
    /// Deserialize a JSON <see cref="string"/> to an <see cref="object"/>
    /// </summary>
    /// <param name="json">The JSON <see cref="string"/> to deserialize (can be implicit)</param>
    /// <typeparam name="T">The type of the <see cref="object"/> to deserialize to (can be implicit)</typeparam>
    /// <returns>The deserialized <see cref="object"/> of the JSON <see cref="string"/></returns>
    /// <seealso cref="ToJson"/>
    public static T? FromJson<T>(this string json) => JsonConvert.DeserializeObject<T>(json);
}