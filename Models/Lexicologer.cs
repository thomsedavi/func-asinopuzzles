using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AsinoPuzzles.Functions.Models
{
    public sealed class LexicologerSummary {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public LexicologerSummary(Lexicologer lexicologer)
        {
            Id = lexicologer.Id;
            Title = lexicologer.Title;
            DateCreated = lexicologer.DateCreated;
            DateUpdated = lexicologer.DateUpdated;
        }
    }

    public sealed class Lexicologer
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public Document Details { get; set; }
        public int? CharacterLimit { get; set; }
        public List<RequiredWord> RequiredWords { get; set;}
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsDeleted { get; set; }
    }

    public sealed class LexicologerResult {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public Document Details { get; set; }
        public int? CharacterLimit { get; set; }
        public List<RequiredWord> RequiredWords { get; set;}
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public LexicologerResult(Lexicologer lexicologer, User user)
        {
            Id = lexicologer.Id;
            UserId = user.Id;
            UserName = user.Name;
            Title = lexicologer.Title;
            Details = lexicologer.Details;
            CharacterLimit = lexicologer.CharacterLimit;
            RequiredWords = lexicologer.RequiredWords;
            DateCreated = lexicologer.DateCreated;
            DateUpdated = lexicologer.DateUpdated;
        }
    }

    public sealed class RequiredWord {
        public string PrimaryWord { get; set; }
        public List<string> SecondaryWords { get; set; }
    }
}
