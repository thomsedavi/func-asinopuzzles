﻿using System;
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

    public sealed class Braider {
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

        public sealed class BraiderResult {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public BraiderResult(Braider braider, User user)
        {
            Id = braider.Id;
            UserId = user.Id;
            UserName = user.Name;
            Title = braider.Title;
            DateCreated = braider.DateCreated;
            DateUpdated = braider.DateUpdated;
        }
    }
}
