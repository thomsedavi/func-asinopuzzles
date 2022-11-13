using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AsinoPuzzles.Functions.Models;
using AsinoPuzzles.Functions.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using User = AsinoPuzzles.Functions.Models.User;

namespace AsinoPuzzles.Functions
{
    public static class UserFunction
    {
        [FunctionName("User")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", "PUT", Route = "user/{id:guid}")] HttpRequest req,
            Guid id, ILogger log)
        {
            try
            {
                var cosmosClient = CosmoClient.New();
                var database = cosmosClient.GetDatabase("AsinoPuzzles");
                var container = database.GetContainer("Users");

                var claimsPrincipal = StaticWebAppsAuth.Parse(req);
                var userId = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (req.Method == "PUT")
                {
                    if (userId == null || userId != id.ToString().ToLower())
                        return new UnauthorizedResult();

                    var userResponse = await container.ReadItemAsync<User>(id.ToString().ToLower(), new PartitionKey(id.ToString().ToLower()));

                    var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    var update = JsonConvert.DeserializeObject<User>(requestBody);

                    var name = update.Name?.Trim() ?? userResponse.Resource.Name ?? "Anonymous";
                    var biography = update.Biography
                        ?? userResponse.Resource.Biography
                        ?? new Document
                        {
                            Sections = new List<Section>
                            {
                                new Section
                                {
                                    Type = "PARAGRAPH",
                                    Element = new Element
                                    {
                                        Text = "Asino Puzzler"
                                    }
                                }
                            }
                        };

                    var biographyJson = JsonConvert.SerializeObject(biography);

                    if (biographyJson.Length > 4000)
                    {
                        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult("BIOGRAPHY_TOO_LONG");
                    }

                    var user = new User
                    {
                        Id = id.ToString().ToLower(),
                        PartitionKey = id.ToString().ToLower(),
                        Name = name[..Math.Min(name.Length, 64)],
                        Biography = biography
                    };

                    await container.ReplaceItemAsync(user, user.Id, new PartitionKey(user.PartitionKey));

                    return new OkObjectResult(user);
                }
                else
                {
                    try
                    {
                        var userResponse = await container.ReadItemAsync<User>(id.ToString().ToLower(), new PartitionKey(id.ToString().ToLower()));

                        return new OkObjectResult(userResponse.Resource);
                    }
                    catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        if (userId == null || userId != id.ToString().ToLower())
                            return new UnauthorizedResult();

                        var user = new User
                        {
                            Id = userId,
                            PartitionKey = userId,
                            Name = "Anonymous",
                            Biography = new Document
                            {
                                Sections = new List<Section>
                                {
                                    new Section
                                    {
                                        Type = "PARAGRAPH",
                                        Element = new Element
                                        {
                                            Text = "Asino Puzzler"
                                        }
                                    }
                                }
                            }
                        };

                        await container.CreateItemAsync(user, new PartitionKey(user.PartitionKey));

                        return new OkObjectResult(user);
                    }
                }
            }
            catch (Exception exception)
            {
                log.LogError(exception, "User Function {Method} {Id} Exception", req.Method, id);
                return new StatusCodeResult(500);
            }
        }
    }
}
