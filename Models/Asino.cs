using System;
using Newtonsoft.Json;

namespace AsinoPuzzles.Functions.Models
{
    public sealed class AsinoSummary {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public AsinoSummary(Asino asino)
        {
            Id = asino.Id;
            Title = asino.Title;
            DateCreated = asino.DateCreated;
            DateUpdated = asino.DateUpdated;
        }
    }

    public sealed class Asino {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsDeleted { get; set; }
    }

    public sealed class AsinoResult {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public AsinoResult(Asino asino, User user)
        {
            Id = asino.Id;
            UserId = user.Id;
            UserName = user.Name;
            Title = asino.Title;
            DateCreated = asino.DateCreated;
            DateUpdated = asino.DateUpdated;
        }
    }
}
