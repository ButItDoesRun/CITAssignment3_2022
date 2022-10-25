
using System.Text.Json.Serialization;

namespace Server
{
    internal class Category
    {
        [JsonPropertyName("cid")]
        public string? Cid { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
