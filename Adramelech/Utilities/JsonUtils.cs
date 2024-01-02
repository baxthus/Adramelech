using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Adramelech.Utilities;

/// <summary>
/// Utility class for JSON
/// </summary>
public static class JsonUtils
{
    private static readonly JsonSerializerSettings DefaultSettings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        }
    };

    /// <summary>
    /// Serialize an <see cref="object"/> to an JSON <see cref="string"/>
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to serialize (can be implicit)</param>
    /// <param name="namingStrategy">The <see cref="NamingStrategy"/> to use for naming (optional)</param>
    /// <returns>The serialized <see cref="string"/> of the <see cref="object"/></returns>
    /// <seealso cref="FromJson{T}(string, NamingStrategy?)"/>
    public static string ToJson(this object obj, NamingStrategy? namingStrategy = null) => JsonConvert.SerializeObject(
        obj, namingStrategy switch
        {
            null => DefaultSettings,
            _ => new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = namingStrategy
                }
            }
        });

    /// <summary>
    /// Deserialize a JSON <see cref="string"/> to an <see cref="object"/>
    /// </summary>
    /// <param name="json">The JSON <see cref="string"/> to deserialize (can be implicit)</param>
    /// <param name="namingStrategy">The <see cref="NamingStrategy"/> to use for naming (optional)</param>
    /// <typeparam name="T">The type of the <see cref="object"/> to deserialize to</typeparam>
    /// <returns>The deserialized <see cref="object"/> of the JSON <see cref="string"/></returns>
    /// <seealso cref="ToJson"/>
    public static T? FromJson<T>(this string json, NamingStrategy? namingStrategy = null) =>
        JsonConvert.DeserializeObject<T>(json, namingStrategy switch
        {
            // Doing this so we don't create a new JsonSerializerSettings if we don't need to
            null => null,
            _ => new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = namingStrategy
                }
            }
        });
}