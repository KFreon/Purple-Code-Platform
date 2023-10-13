using Microsoft.Azure.Cosmos;

public class CosmosDbService
{
    public const string CosmosDbName = "purple-code-platform";
    public const string SnippetContainerName = "snippets";
    public const string SnippetsPartitionKey = "/id";

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
            LimitToEndpoint = true
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

    public async Task<IReadOnlyList<T>> Get<T>()
    {
        var (container, client) = await GetContainer(CosmosDbName, SnippetContainerName, SnippetsPartitionKey);
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

        return items;
    }

    public async Task Upsert(Snippet snippet)
    {
        var (container, client) = await GetContainer(CosmosDbName, SnippetContainerName, SnippetsPartitionKey);
        await container.UpsertItemAsync(snippet, new PartitionKey(snippet.Id));
        client.Dispose();
    }
}