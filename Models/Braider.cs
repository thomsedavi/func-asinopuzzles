using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AsinoPuzzles.Functions.Models
{
    public sealed class BraiderSummary {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public BraiderSummary(Braider braider)
        {
            Id = braider.Id;
            Title = braider.Title;
            DateCreated = braider.DateCreated;
            DateUpdated = braider.DateUpdated;
        }
    }

    public sealed class Braider {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public List<Variable> Variables { get; set;}
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsDeleted { get; set; }
    }

    public sealed class BraiderResult {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public List<Variable> Variables { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public BraiderResult(Braider braider, User user)
        {
            Id = braider.Id;
            UserId = user.Id;
            UserName = user.Name;
            Title = braider.Title;
            Variables = braider.Variables;
            DateCreated = braider.DateCreated;
            DateUpdated = braider.DateUpdated;
        }
    }

    public sealed class BraiderStyle {
        public string FontWeight { get; set; }
        public string FontStyle { get; set; }
    }

    public sealed class BraiderSpan {
        public string Type { get; set; }
        public List<BraiderSpan> Spans { get; set; }
        public BraiderStyle Style { get; set; }
        public string IsVariableId { get; set; }
        public string PageId { get; set; }
        public string Value { get; set; }
        public string VariableId { get; set; }
    }

    public sealed class BraiderSelectOptionString {
        public string Id { get; set; }
        public List<BraiderSpan> Spans { get; set; }
        public string IsVariableId { get; set; }
    }

    public sealed class Variable {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
        public string Expression { get; set; }
        public string VariableId { get; set; }
        public string DefaultOptionId { get; set; }
        public string DefaultValue { get; set; }
        public List<BraiderSelectOptionString> Options { get; set; }
        public string OptionId { get; set; }
        public string Value { get; set; }
    }
}
