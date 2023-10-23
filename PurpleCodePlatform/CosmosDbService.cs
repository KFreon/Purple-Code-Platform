using System.Text.Json;
using Microsoft.Azure.Cosmos;
using PurpleCodePlatform;

// Probably just have different methods?
// Less easy to extend I guess
// Perhaps different implementations based on this one
public interface CosmosItemInfo
{
    public string PartitionKeyName { get; }
    public string ContainerName { get;  }
    public PartitionKey PartitionKey => new(PartitionKeyName);
}

public class CosmosSnippetInfo : CosmosItemInfo
{
    public string PartitionKeyName => "/id";
    public string ContainerName => "snippets";
}

public class CosmoUserInfo : CosmosItemInfo
{
    public string PartitionKeyName => "/id";
    public string ContainerName => "users";
}

public class CosmosDbService
{
    public const string CosmosDbName = "purple-code-platform";

    public static CosmosItemInfo UserInfo = new CosmoUserInfo();
    public static CosmosItemInfo SnippetInfo = new CosmosSnippetInfo();

    public string Endpoint { get; init; }
    public string Key { get; init; }
    public string Environment { get;init; }

    public CosmosDbService(string endpoint, string key, string environment)
    {
        Endpoint = endpoint;
        Key = key;
        Environment = environment;
    }

    private CosmosClient GetClient()
    {
        CosmosClientOptions options = Environment == "Development" ? new()
        {
            HttpClientFactory = () => new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }),
            ConnectionMode = ConnectionMode.Gateway,
            LimitToEndpoint = true,
            Serializer = new CosmosSystemTextJsonSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web))
        } : new();

        var cosmosClient = new CosmosClient(
            accountEndpoint: Endpoint,
            authKeyOrResourceToken: Key,
            options
        );

        return cosmosClient;
    }

    private async Task<(Container Container, CosmosClient Client)> GetContainer(string dbName, string containerName, string partition)
    {
        var cosmosClient = GetClient();
        Database db = await cosmosClient.CreateDatabaseIfNotExistsAsync(id: dbName);
        Container container = await db.CreateContainerIfNotExistsAsync(
            id: containerName,
            partitionKeyPath: partition);

        return (container, cosmosClient);
    }

    public async Task<Result<T>> Get<T>(string id, CosmosItemInfo info)
    {
        var (container, client) = await GetContainer(CosmosDbName, info.ContainerName, info.PartitionKeyName);
        try
        {
            var item = await container.ReadItemAsync<T>(id, info.PartitionKey);
            return Result.Ok<T>(item);
        }
        catch (Exception ex) when (ex.Message.Contains("NotFound"))
        {
            return Result.NotFound<T>();
        }
        finally
        {
            client.Dispose();
        }
    }

    public async Task<Result<IReadOnlyList<T>>> GetAll<T>(CosmosItemInfo info)
    {
        var (container, client) = await GetContainer(CosmosDbName, info.ContainerName, info.PartitionKeyName);
        List<T> items = new();
        var iterator = container.GetItemQueryIterator<T>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            foreach (var item in response)
            {
                items.Add(item);
            }
        }

        // Dunno if this is actually necessary, but I'll be a good citizen.
        client.Dispose();

        IReadOnlyList<T> result = items.AsReadOnly();
        return Result.Ok(result);
    }

    public async Task Upsert<T>(T item, CosmosItemInfo info)
    {
        var (container, client) = await GetContainer(CosmosDbName, info.ContainerName, info.PartitionKeyName);
        await container.UpsertItemAsync(item);
        client.Dispose();
    }

    public async Task Delete<T>(string id, string language, CosmosItemInfo info)
    {
        var (container, client) = await GetContainer(CosmosDbName, info.ContainerName, info.PartitionKeyName);
        try
        {
            await container.DeleteItemAsync<T>(id, info.PartitionKey);
        } 
        catch(Exception ex) when (ex.Message.Contains("NotFound"))
        {
            // Not found, don't really care about it then
        }
        client.Dispose();
    }
}
