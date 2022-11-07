using System;
using Microsoft.Azure.Cosmos;

namespace AsinoPuzzles.Functions.Utils
{
    public sealed class CosmoClient
    {
        public static CosmosClient New()
        {
            return new CosmosClient(
                Environment.GetEnvironmentVariable("EndPointUri", EnvironmentVariableTarget.Process),
                Environment.GetEnvironmentVariable("PrimaryKey", EnvironmentVariableTarget.Process),
                new CosmosClientOptions()
                {
                    ApplicationName = "AsinoPuzzlesFunction"
                });
        }
    }
}
