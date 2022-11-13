using Newtonsoft.Json;

namespace AsinoPuzzles.Functions.Models
{
    public sealed class User
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        public string Name { get; set; }
        public Document Biography { get; set; }
    }
}
