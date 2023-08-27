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
    public static class AsinoCreateFunction {
        [FunctionName("AsinoCreate")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "asinoes")] HttpRequest req, ILogger log) {
            try
            {
                var cosmosClient = CosmoClient.New();
                var database = cosmosClient.GetDatabase("AsinoPuzzles");
                var usersContainer = database.GetContainer("Users");
                var userIdsContainer = database.GetContainer("UserIds");
                var asinoesContainer = database.GetContainer("Asinoes");

                var claimsPrincipal = StaticWebAppsAuth.Parse(req);
                var claimId = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                // gotta be logged in!
                if (claimId == null)
                    return new UnauthorizedResult();

                try
                {
                    var userIdResponse = await userIdsContainer.ReadItemAsync<UserIdObject>(claimId.ToLower(), new PartitionKey(claimId.ToLower()));
                    var userId = userIdResponse.Resource.UserId;

                    var attempt = 1;
                    var asinoPuzzleId = IdUtils.CreateRandomId(attempt);
                    var okay = false;

                    while (!okay && attempt < 20)
                    {
                        try
                        {
                            var asinoResponse = await usersContainer.ReadItemAsync<Asino>(asinoPuzzleId, new PartitionKey(asinoPuzzleId));

                            attempt++;
                            asinoPuzzleId = IdUtils.CreateRandomId(attempt);
                        }
                        catch (CosmosException __) when (__.StatusCode == HttpStatusCode.NotFound)
                        {
                            okay = true;
                        }
                    }

                    if (okay)
                    {
                        var userResponse = await usersContainer.ReadItemAsync<User>(userId, new PartitionKey(userId));
                        var user = userResponse.Resource;

                        var asinoIds = user.AsinoIds ?? new List<string>();
                        asinoIds.Add(asinoPuzzleId);
                        user.AsinoIds = asinoIds;

                        await usersContainer.ReplaceItemAsync(user, user.Id, new PartitionKey(user.PartitionKey));

                        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                        var newPuzzle = JsonConvert.DeserializeObject<Braider>(requestBody);

                        var title = newPuzzle.Title?.Trim() ?? "Asino Puzzle";
                        // other things go here

                        var asino = new Asino
                        {
                            Id = asinoPuzzleId,
                            PartitionKey = asinoPuzzleId,
                            UserId = userId,
                            Title = title[..Math.Min(title.Length, 64)],
                            DateCreated = DateTime.UtcNow,
                            DateUpdated = DateTime.UtcNow
                        };

                        await asinoesContainer.CreateItemAsync(asino, new PartitionKey(asino.PartitionKey));

                        return new OkObjectResult(new AsinoResult(asino, user));
                    }
                    else
                    {
                        log.LogError("Unable to create a distinct Asino Id");
                        return new StatusCodeResult(500);
                    }
                }
                catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    log.LogError(exception, "Bad {ClaimId}", claimId);
                    return new StatusCodeResult(500);
                }
            }
            catch (Exception exception)
            {
                log.LogError(exception, "Asino Function {Method} Exception", req.Method);
                return new StatusCodeResult(500);
            }
        }
    }


    public static class BraiderCreateFunction {
        [FunctionName("BraiderCreate")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "braiders")] HttpRequest req, ILogger log)
        {
            try
            {
                var cosmosClient = CosmoClient.New();
                var database = cosmosClient.GetDatabase("AsinoPuzzles");
                var usersContainer = database.GetContainer("Users");
                var userIdsContainer = database.GetContainer("UserIds");
                var braidersContainer = database.GetContainer("Braiders");

                var claimsPrincipal = StaticWebAppsAuth.Parse(req);
                var claimId = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                // gotta be logged in!
                if (claimId == null)
                    return new UnauthorizedResult();

                try
                {
                    var userIdResponse = await userIdsContainer.ReadItemAsync<UserIdObject>(claimId.ToLower(), new PartitionKey(claimId.ToLower()));
                    var userId = userIdResponse.Resource.UserId;

                    var attempt = 1;
                    var braiderGameId = IdUtils.CreateRandomId(attempt);
                    var okay = false;

                    while (!okay && attempt < 20)
                    {
                        try
                        {
                            var braiderResponse = await usersContainer.ReadItemAsync<Braider>(braiderGameId, new PartitionKey(braiderGameId));

                            attempt++;
                            braiderGameId = IdUtils.CreateRandomId(attempt);
                        }
                        catch (CosmosException __) when (__.StatusCode == HttpStatusCode.NotFound)
                        {
                            okay = true;
                        }
                    }

                    if (okay)
                    {
                        var userResponse = await usersContainer.ReadItemAsync<User>(userId, new PartitionKey(userId));
                        var user = userResponse.Resource;

                        var braiderIds = user.BraiderIds ?? new List<string>();
                        braiderIds.Add(braiderGameId);
                        user.BraiderIds = braiderIds;

                        await usersContainer.ReplaceItemAsync(user, user.Id, new PartitionKey(user.PartitionKey));

                        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                        var newGame = JsonConvert.DeserializeObject<Braider>(requestBody);

                        var title = newGame.Title?.Trim() ?? "Braider Game";
                        var variables = BraiderFunctions.ValidateVariables(newGame.Variables);
                        var elements = BraiderFunctions.ValidateElements(newGame.Elements, variables);
                        // other things go here

                        var braider = new Braider
                        {
                            Id = braiderGameId,
                            PartitionKey = braiderGameId,
                            UserId = userId,
                            Title = title[..Math.Min(title.Length, 64)],
                            Variables = variables,
                            Elements = elements,
                            DateCreated = DateTime.UtcNow,
                            DateUpdated = DateTime.UtcNow
                        };

                        await braidersContainer.CreateItemAsync(braider, new PartitionKey(braider.PartitionKey));

                        return new OkObjectResult(new BraiderResult(braider, user));
                    }
                    else
                    {
                        log.LogError("Unable to create a distinct Braider Id");
                        return new StatusCodeResult(500);
                    }
                }
                catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    log.LogError(exception, "Bad {ClaimId}", claimId);
                    return new StatusCodeResult(500);
                }
            }
            catch (Exception exception)
            {
                log.LogError(exception, "Braider Function {Method} Exception", req.Method);
                return new StatusCodeResult(500);
            }
        }
    }

    public static class BraiderUpdateFunction {
        [FunctionName("BraiderUpdate")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "GET", "PUT", "DELETE", Route = "braiders/{id}")] HttpRequest req, string id, ILogger log)
        {
            try
            {
                var cosmosClient = CosmoClient.New();
                var database = cosmosClient.GetDatabase("AsinoPuzzles");
                var usersContainer = database.GetContainer("Users");
                var userIdsContainer = database.GetContainer("UserIds");
                var braidersContainer = database.GetContainer("Braiders");

                var claimsPrincipal = StaticWebAppsAuth.Parse(req);
                var claimId = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var braiderResponse = await braidersContainer.ReadItemAsync<Braider>(id.ToLower(), new PartitionKey(id.ToLower()));
                var braider = braiderResponse.Resource;

                var userResponse = await usersContainer.ReadItemAsync<User>(braider.UserId.ToLower(), new PartitionKey(braider.UserId.ToLower()));
                var user = userResponse.Resource;

                if (req.Method == "GET") {
                    if (braider.IsDeleted)
                        return new StatusCodeResult(500);

                    return new OkObjectResult(new BraiderResult(braider, user));
                } else if (req.Method == "PUT") {
                    var userIdResponse = await userIdsContainer.ReadItemAsync<UserIdObject>(claimId.ToLower(), new PartitionKey(claimId.ToLower()));
                    var userId = userIdResponse.Resource.UserId;

                    if (braider.UserId != userId)
                        return new UnauthorizedResult();

                    var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    var update = JsonConvert.DeserializeObject<Braider>(requestBody);

                    var title = update.Title?.Trim() ?? braider.Title;
                    var variables = update.Variables != null ? BraiderFunctions.ValidateVariables(update.Variables) : braider.Variables;
                    var elements = update.Elements != null ? BraiderFunctions.ValidateElements(update.Elements, update.Variables) : braider.Elements;
                    // other things go here

                    braider.Title = title[..Math.Min(title.Length, 64)];
                    braider.Variables = variables;
                    braider.Elements = elements;
                    braider.DateUpdated = DateTime.UtcNow;

                    await braidersContainer.ReplaceItemAsync(braider, braider.Id, new PartitionKey(braider.PartitionKey));
                    
                    return new OkObjectResult(new BraiderResult(braider, user));
                } else if (req.Method == "DELETE") {
                    var userIdResponse = await userIdsContainer.ReadItemAsync<UserIdObject>(claimId.ToLower(), new PartitionKey(claimId.ToLower()));
                    var userId = userIdResponse.Resource.UserId;

                    if (braider.UserId != userId)
                        return new UnauthorizedResult();

                    braider.IsDeleted = true;

                    await braidersContainer.ReplaceItemAsync(braider, braider.Id, new PartitionKey(braider.PartitionKey));
                    
                    return new OkResult();
                } else {
                    return new StatusCodeResult(500);
                }
            }
            catch (Exception exception)
            {
                log.LogError(exception, "Braider Function {Method} {Id} Exception", req.Method, id);
                return new StatusCodeResult(500);
            }
        }
    }

    public static class LexicologersCreateFunction {
        [FunctionName("LexicologerCreate")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "lexicologers")] HttpRequest req, ILogger log)
        {
            try
            {
                var cosmosClient = CosmoClient.New();
                var database = cosmosClient.GetDatabase("AsinoPuzzles");
                var usersContainer = database.GetContainer("Users");
                var userIdsContainer = database.GetContainer("UserIds");
                var lexicologersContainer = database.GetContainer("Lexicologers");

                var claimsPrincipal = StaticWebAppsAuth.Parse(req);
                var claimId = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                // gotta be logged in!
                if (claimId == null)
                    return new UnauthorizedResult();

                try
                {
                    var userIdResponse = await userIdsContainer.ReadItemAsync<UserIdObject>(claimId.ToLower(), new PartitionKey(claimId.ToLower()));
                    var userId = userIdResponse.Resource.UserId;

                    var attempt = 1;
                    var lexicologerGameId = IdUtils.CreateRandomId(attempt);
                    var okay = false;

                    while (!okay && attempt < 20)
                    {
                        try
                        {
                            var lexicologerResponse = await usersContainer.ReadItemAsync<Lexicologer>(lexicologerGameId, new PartitionKey(lexicologerGameId));

                            attempt++;
                            lexicologerGameId = IdUtils.CreateRandomId(attempt);
                        }
                        catch (CosmosException __) when (__.StatusCode == HttpStatusCode.NotFound)
                        {
                            okay = true;
                        }
                    }

                    if (okay)
                    {
                        var userResponse = await usersContainer.ReadItemAsync<User>(userId, new PartitionKey(userId));
                        var user = userResponse.Resource;

                        var lexicologerIds = user.LexicologerIds ?? new List<string>();
                        lexicologerIds.Add(lexicologerGameId);
                        user.LexicologerIds = lexicologerIds;

                        await usersContainer.ReplaceItemAsync(user, user.Id, new PartitionKey(user.PartitionKey));

                        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                        var newGame = JsonConvert.DeserializeObject<Lexicologer>(requestBody);

                        var title = newGame.Title?.Trim() ?? "Lexicologer Game";
                        var details = newGame.Details ?? new Document
                            {
                                Sections = new List<Section>
                                {
                                    new Section
                                    {
                                        Type = "PARAGRAPH",
                                        Element = new Element
                                        {
                                            Text = "Try to write something within the character limit that makes use of all the words listed below"
                                        }
                                    }
                                }
                            };
                        var requiredWords = newGame.RequiredWords ?? new List<RequiredWord>();

                        var detailsJson = JsonConvert.SerializeObject(details);
                        var requiredWordsJson = JsonConvert.SerializeObject(requiredWords);
                    
                        if (detailsJson.Length > 4000 || requiredWordsJson.Length > 4000)
                        {
                            return new BadRequestObjectResult("Too long");
                        }

                        var lexicologer = new Lexicologer
                        {
                            Id = lexicologerGameId,
                            PartitionKey = lexicologerGameId,
                            UserId = userId,
                            Title = title[..Math.Min(title.Length, 64)],
                            Details = details,
                            CharacterLimit = newGame.CharacterLimit ?? 140,
                            RequiredWords = newGame.RequiredWords ?? new List<RequiredWord>(),
                            DateCreated = DateTime.UtcNow,
                            DateUpdated = DateTime.UtcNow
                        };

                        await lexicologersContainer.CreateItemAsync(lexicologer, new PartitionKey(lexicologer.PartitionKey));

                        return new OkObjectResult(new LexicologerResult(lexicologer, user));
                    }
                    else
                    {
                        log.LogError("Unable to create a distinct Lexicologer Id");
                        return new StatusCodeResult(500);
                    }
                }
                catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    log.LogError(exception, "Bad {ClaimId}", claimId);
                    return new StatusCodeResult(500);
                }
            }
            catch (Exception exception)
            {
                log.LogError(exception, "Lexicologer Function {Method} Exception", req.Method);
                return new StatusCodeResult(500);
            }
        }
    }

    public static class LexicologersUpdateFunction {
        [FunctionName("LexicologerUpdate")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "GET", "PUT", "DELETE", Route = "lexicologers/{id}")] HttpRequest req, string id, ILogger log)
        {
            try
            {
                var cosmosClient = CosmoClient.New();
                var database = cosmosClient.GetDatabase("AsinoPuzzles");
                var usersContainer = database.GetContainer("Users");
                var userIdsContainer = database.GetContainer("UserIds");
                var lexicologersContainer = database.GetContainer("Lexicologers");

                var claimsPrincipal = StaticWebAppsAuth.Parse(req);
                var claimId = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var lexicologerResponse = await lexicologersContainer.ReadItemAsync<Lexicologer>(id.ToLower(), new PartitionKey(id.ToLower()));
                var lexicologer = lexicologerResponse.Resource;

                var userResponse = await usersContainer.ReadItemAsync<User>(lexicologer.UserId.ToLower(), new PartitionKey(lexicologer.UserId.ToLower()));
                var user = userResponse.Resource;

                if (req.Method == "GET") {
                    if (lexicologer.IsDeleted)
                        return new StatusCodeResult(500);

                    return new OkObjectResult(new LexicologerResult(lexicologer, user));
                } else if (req.Method == "PUT") {
                    var userIdResponse = await userIdsContainer.ReadItemAsync<UserIdObject>(claimId.ToLower(), new PartitionKey(claimId.ToLower()));
                    var userId = userIdResponse.Resource.UserId;

                    if (lexicologer.UserId != userId)
                        return new UnauthorizedResult();

                    var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    var update = JsonConvert.DeserializeObject<Lexicologer>(requestBody);

                    var title = update.Title?.Trim() ?? lexicologer.Title;
                    var details = update.Details ?? lexicologer.Details;
                    var requiredWords = update.RequiredWords ?? lexicologer.RequiredWords;

                    var detailsJson = JsonConvert.SerializeObject(details);
                    var requiredWordsJson = JsonConvert.SerializeObject(requiredWords);
                    
                    if (detailsJson.Length > 4000 || requiredWordsJson.Length > 4000)
                    {
                        return new BadRequestObjectResult("Too long");
                    }

                    lexicologer.Title = title[..Math.Min(title.Length, 64)];
                    lexicologer.Details = details;
                    lexicologer.CharacterLimit = update.CharacterLimit ?? lexicologer.CharacterLimit;
                    lexicologer.RequiredWords = requiredWords;
                    lexicologer.DateUpdated = DateTime.UtcNow;

                    await lexicologersContainer.ReplaceItemAsync(lexicologer, lexicologer.Id, new PartitionKey(lexicologer.PartitionKey));
                    
                    return new OkObjectResult(new LexicologerResult(lexicologer, user));
                } else if (req.Method == "DELETE") {
                    var userIdResponse = await userIdsContainer.ReadItemAsync<UserIdObject>(claimId.ToLower(), new PartitionKey(claimId.ToLower()));
                    var userId = userIdResponse.Resource.UserId;

                    if (lexicologer.UserId != userId)
                        return new UnauthorizedResult();

                    lexicologer.IsDeleted = true;

                    await lexicologersContainer.ReplaceItemAsync(lexicologer, lexicologer.Id, new PartitionKey(lexicologer.PartitionKey));
                    
                    return new OkResult();
                } else {
                    return new StatusCodeResult(500);
                }
            }
            catch (Exception exception)
            {
                log.LogError(exception, "Lexicologer Function {Method} {Id} Exception", req.Method, id);
                return new StatusCodeResult(500);
            }
        }
    }

    public static class UserFunction
    {
        [FunctionName("User")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "GET", "PUT", Route = "users/{id}")] HttpRequest req, string id, ILogger log)
        {
            try
            {
                var cosmosClient = CosmoClient.New();
                var database = cosmosClient.GetDatabase("AsinoPuzzles");
                var usersContainer = database.GetContainer("Users");
                var userIdsContainer = database.GetContainer("UserIds");
                var lexicologersContainer = database.GetContainer("Lexicologers");
                var braidersContainer = database.GetContainer("Braiders");
                var asinoesContainer = database.GetContainer("Asinoes");

                var claimsPrincipal = StaticWebAppsAuth.Parse(req);
                var claimId = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                // we are getting the user by their client id
                if (claimId != null && id.ToLower() == claimId.ToLower())
                {
                    try
                    {
                        var userIdResponse = await userIdsContainer.ReadItemAsync<UserIdObject>(id.ToString().ToLower(), new PartitionKey(id.ToString().ToLower()));

                        // you are an existing user, here are your details
                        try
                        {
                            var userResponse = await usersContainer.ReadItemAsync<User>(userIdResponse.Resource.UserId, new PartitionKey(userIdResponse.Resource.UserId));
                            var user = userResponse.Resource;

                            var userResult = new UserResult(user);

                            if (user.LexicologerIds != null && user.LexicologerIds.Any()) {
                                try
                                {
                                    var lexicologerList = user.LexicologerIds.Select(lexicologerId => (lexicologerId, new PartitionKey(lexicologerId))).ToList();

                                    var lexicologersResponse = await lexicologersContainer.ReadManyItemsAsync<Lexicologer>(lexicologerList);
                                    var lexicologers = lexicologersResponse.Resource.Where(lexicologer => !lexicologer.IsDeleted);

                                    userResult.Lexicologers = lexicologers.Select(lexicologer => new LexicologerSummary(lexicologer)).ToList();
                                }
                                catch (Exception exception)
                                {
                                    log.LogError(exception, "Lexicologers error for {UserId}", user.Id);
                                }
                            }

                            if (user.BraiderIds != null && user.BraiderIds.Any()) {
                                try
                                {
                                    var braiderList = user.BraiderIds.Select(braiderId => (braiderId, new PartitionKey(braiderId))).ToList();

                                    var braidersResponse = await braidersContainer.ReadManyItemsAsync<Braider>(braiderList);
                                    var braiders = braidersResponse.Resource.Where(braider => !braider.IsDeleted);

                                    userResult.Braiders = braiders.Select(braider => new BraiderSummary(braider)).ToList();
                                }
                                catch (Exception exception)
                                {
                                    log.LogError(exception, "Braiders error for {UserId}", user.Id);
                                }
                            }

                            if (user.AsinoIds != null && user.AsinoIds.Any()) {
                                try
                                {
                                    var asinoList = user.AsinoIds.Select(asinoId => (asinoId, new PartitionKey(asinoId))).ToList();

                                    var asinoesResponse = await asinoesContainer.ReadManyItemsAsync<Asino>(asinoList);
                                    var asinoes = asinoesResponse.Resource.Where(asino => !asino.IsDeleted);

                                    userResult.Asinoes = asinoes.Select(asino => new AsinoSummary(asino)).ToList();
                                }
                                catch (Exception exception)
                                {
                                    log.LogError(exception, "Asinoes error for {UserId}", user.Id);
                                }
                            }

                            return new OkObjectResult(userResult);
                        }
                        catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
                        {
                            log.LogError(exception, "Unable to find {UserId} for {ClientId}", userIdResponse.Resource.UserId, id);
                            return new StatusCodeResult(500);
                        }
                    }
                    catch (CosmosException _) when (_.StatusCode == HttpStatusCode.NotFound)
                    {
                        // you must be new here! try to find an Id that hasn't been used yet
                        var attempt = 1;
                        var userId = IdUtils.CreateRandomId(attempt);
                        var okay = false;

                        while (!okay && attempt < 20)
                        {
                            try
                            {
                                var userResponse = await usersContainer.ReadItemAsync<User>(userId, new PartitionKey(userId));

                                attempt++;
                                userId = IdUtils.CreateRandomId(attempt);
                            }
                            catch (CosmosException __) when (__.StatusCode == HttpStatusCode.NotFound)
                            {
                                okay = true;
                            }
                        }

                        if (okay)
                        {
                            var userIdObject = new UserIdObject
                            {
                                Id = id,
                                PartitionKey = id,
                                UserId = userId,
                            };

                            await userIdsContainer.CreateItemAsync(userIdObject, new PartitionKey(userIdObject.PartitionKey));

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
                                },
                                DateCreated = DateTime.UtcNow,
                                DateUpdated = DateTime.UtcNow
                            };

                            await usersContainer.CreateItemAsync(user, new PartitionKey(user.PartitionKey));

                            var userResult = new UserResult(user);

                            if (user.LexicologerIds != null && user.LexicologerIds.Any()) {
                                try
                                {
                                    var lexicologerList = user.LexicologerIds.Select(lexicologerId => (lexicologerId, new PartitionKey(lexicologerId))).ToList();

                                    var lexicologersResponse = await lexicologersContainer.ReadManyItemsAsync<Lexicologer>(lexicologerList);
                                    var lexicologers = lexicologersResponse.Resource.Where(lexicologer => !lexicologer.IsDeleted);

                                    userResult.Lexicologers = lexicologers.Select(lexicologer => new LexicologerSummary(lexicologer)).ToList();
                                }
                                catch (Exception exception)
                                {
                                    log.LogError(exception, "Lexicologers error for {UserId}", user.Id);
                                }
                            }

                            if (user.BraiderIds != null && user.BraiderIds.Any()) {
                                try
                                {
                                    var braiderList = user.BraiderIds.Select(braiderId => (braiderId, new PartitionKey(braiderId))).ToList();

                                    var braidersResponse = await braidersContainer.ReadManyItemsAsync<Braider>(braiderList);
                                    var braiders = braidersResponse.Resource.Where(braider => !braider.IsDeleted);

                                    userResult.Braiders = braiders.Select(braider => new BraiderSummary(braider)).ToList();
                                }
                                catch (Exception exception)
                                {
                                    log.LogError(exception, "Braiders error for {UserId}", user.Id);
                                }
                            }

                            if (user.AsinoIds != null && user.AsinoIds.Any()) {
                                try
                                {
                                    var asinoList = user.AsinoIds.Select(asinoId => (asinoId, new PartitionKey(asinoId))).ToList();

                                    var asinoesResponse = await asinoesContainer.ReadManyItemsAsync<Asino>(asinoList);
                                    var asinoes = asinoesResponse.Resource.Where(asino => !asino.IsDeleted);

                                    userResult.Asinoes = asinoes.Select(asino => new AsinoSummary(asino)).ToList();
                                }
                                catch (Exception exception)
                                {
                                    log.LogError(exception, "Asinoes error for {UserId}", user.Id);
                                }
                            }

                            return new OkObjectResult(userResult);
                        }
                        else
                        {
                            log.LogError("Unable to create a distinct User Id");
                            return new StatusCodeResult(500);
                        }
                    }
                }
                else if (req.Method == "GET")
                {
                    try
                    {
                        var userResponse = await usersContainer.ReadItemAsync<User>(id, new PartitionKey(id));

                        var userResult = new UserResult(userResponse.Resource);
                        var user = userResponse.Resource;

                        if (user.LexicologerIds != null && user.LexicologerIds.Any()) {
                            try
                            {
                                var lexicologerList = user.LexicologerIds.Select(lexicologerId => (lexicologerId, new PartitionKey(lexicologerId))).ToList();

                                var lexicologersResponse = await lexicologersContainer.ReadManyItemsAsync<Lexicologer>(lexicologerList);
                                var lexicologers = lexicologersResponse.Resource.Where(lexicologer => !lexicologer.IsDeleted);

                                userResult.Lexicologers = lexicologers.Select(lexicologer => new LexicologerSummary(lexicologer)).ToList();
                            }
                            catch (Exception exception)
                            {
                                log.LogError(exception, "Lexicologers error for {UserId}", user.Id);
                            }
                        }

                        if (user.BraiderIds != null && user.BraiderIds.Any()) {
                            try
                            {
                                var braiderList = user.BraiderIds.Select(braiderId => (braiderId, new PartitionKey(braiderId))).ToList();

                                var braidersResponse = await braidersContainer.ReadManyItemsAsync<Braider>(braiderList);
                                var braiders = braidersResponse.Resource.Where(braider => !braider.IsDeleted);

                                userResult.Braiders = braiders.Select(braider => new BraiderSummary(braider)).ToList();
                            }
                            catch (Exception exception)
                            {
                                log.LogError(exception, "Braiders error for {UserId}", user.Id);
                            }
                        }

                        if (user.AsinoIds != null && user.AsinoIds.Any()) {
                            try
                            {
                                var asinoList = user.AsinoIds.Select(asinoId => (asinoId, new PartitionKey(asinoId))).ToList();

                                var asinoesResponse = await asinoesContainer.ReadManyItemsAsync<Asino>(asinoList);
                                var asinoes = asinoesResponse.Resource.Where(asino => !asino.IsDeleted);

                                userResult.Asinoes = asinoes.Select(asino => new AsinoSummary(asino)).ToList();
                            }
                            catch (Exception exception)
                            {
                                log.LogError(exception, "Asinoes error for {UserId}", user.Id);
                            }
                        }

                        return new OkObjectResult(userResult);
                    }
                    catch (CosmosException __) when (__.StatusCode == HttpStatusCode.NotFound)
                    {
                        log.LogError("Unable to get User with Id {UserId}", id);
                        return new StatusCodeResult(500);
                    }
                }
                else if (req.Method == "PUT")
                {
                    // gotta be logged in!
                    if (claimId == null)
                        return new UnauthorizedResult();

                    try
                    {
                        var userIdResponse = await userIdsContainer.ReadItemAsync<UserIdObject>(claimId, new PartitionKey(claimId));

                        // Dennis Nedry 'uh uh uh' gif
                        if (userIdResponse.Resource.UserId != id)
                            return new UnauthorizedResult();

                        var userResponse = await usersContainer.ReadItemAsync<User>(id, new PartitionKey(id));
                        var user = userResponse.Resource;

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
                            return new BadRequestObjectResult("Too long");
                        }

                        user.Name = name[..Math.Min(name.Length, 64)];
                        user.Biography = biography;
                        user.DateUpdated = DateTime.UtcNow;
                                        
                        await usersContainer.ReplaceItemAsync(user, user.Id, new PartitionKey(user.PartitionKey));
                        
                        var userResult = new UserResult(user);

                        if (user.LexicologerIds != null && user.LexicologerIds.Any()) {
                            try
                            {
                                var lexicologerList = user.LexicologerIds.Select(lexicologerId => (lexicologerId, new PartitionKey(lexicologerId))).ToList();

                                var lexicologersResponse = await lexicologersContainer.ReadManyItemsAsync<Lexicologer>(lexicologerList);
                                var lexicologers = lexicologersResponse.Resource.Where(lexicologer => !lexicologer.IsDeleted);

                                userResult.Lexicologers = lexicologers.Select(lexicologer => new LexicologerSummary(lexicologer)).ToList();
                            }
                            catch (Exception exception)
                            {
                                log.LogError(exception, "Lexicologers error for {UserId}", user.Id);
                            }
                        }

                        if (user.BraiderIds != null && user.BraiderIds.Any()) {
                            try
                            {
                                var braiderList = user.BraiderIds.Select(braiderId => (braiderId, new PartitionKey(braiderId))).ToList();

                                var braidersResponse = await braidersContainer.ReadManyItemsAsync<Braider>(braiderList);
                                var braiders = braidersResponse.Resource.Where(braider => !braider.IsDeleted);

                                userResult.Braiders = braiders.Select(braider => new BraiderSummary(braider)).ToList();
                            }
                            catch (Exception exception)
                            {
                                log.LogError(exception, "Braiders error for {UserId}", user.Id);
                            }
                        }

                        if (user.AsinoIds != null && user.AsinoIds.Any()) {
                            try
                            {
                                var asinoList = user.AsinoIds.Select(asinoId => (asinoId, new PartitionKey(asinoId))).ToList();

                                var asinoesResponse = await asinoesContainer.ReadManyItemsAsync<Asino>(asinoList);
                                var asinoes = asinoesResponse.Resource.Where(asino => !asino.IsDeleted);

                                userResult.Asinoes = asinoes.Select(asino => new AsinoSummary(asino)).ToList();
                            }
                            catch (Exception exception)
                            {
                                log.LogError(exception, "Asinoes error for {UserId}", user.Id);
                            }
                        }

                        return new OkObjectResult(userResult);
                    }
                    catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
                    {
                        log.LogError(exception, "Unable to find {UserId} for {ClientId}", id, claimId);
                        return new StatusCodeResult(500);
                    }
                }

                throw new Exception("Unsupported action?");
            }
            catch (Exception exception)
            {
                log.LogError(exception, "User Function {Method} {Id} Exception", req.Method, id);
                return new StatusCodeResult(500);
            }
        }
    }
}
