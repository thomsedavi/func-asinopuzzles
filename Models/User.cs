using System;
using System.Collections.Generic;
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
        public List<string> LexicologerIds { get; set; }
        public List<string> BraiderIds { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }

    public sealed class UserResult {
        public string Id { get; set; }
        public string Name { get; set; }
        public Document Biography { get; set; }
        public List<LexicologerSummary> Lexicologers { get; set; }
        public List<BraiderSummary> Braiders { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public UserResult(User user)
        {
            Id = user.Id;
            Name = user.Name;
            Biography = user.Biography;
            DateCreated = user.DateCreated;
            DateUpdated = user.DateUpdated;
        }
    }

    public sealed class UserIdObject
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        public string UserId { get; set; }
    }
}
