using Azure;
using Azure.Data.Tables;
using Bogus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace PokeTrader.Api.SellOrders;

public class SeedSellOrdersBatch
{
    private readonly ILogger<GetSellOrders> _logger;
    private readonly TableServiceClient _tableService;

    private const string partitionKey = "pokemon_sell_order";
    public SeedSellOrdersBatch(ILogger<GetSellOrders> logger, TableServiceClient tableService)
    {
        _logger = logger;
        _tableService = tableService;
    }

    [Function("SeedPokemonSellOrdersBatch")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] 
        HttpRequestData req, 
        CancellationToken ct)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        try
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string batchSizeStr = query["batchSize"];
            bool.TryParse(query["isWhenAll"], out bool isWhenAll);
            
            var tableClient = _tableService.GetTableClient("sellorders");

            var sellOrderList = GetFakeSellOrders(1000);

            // create batch
            List<TableTransactionAction> addEntitiesBatch = new();

            foreach (var entity in sellOrderList)
            {
                addEntitiesBatch.Add(new TableTransactionAction(TableTransactionActionType.Add, entity));
            }

            var startTime = Stopwatch.GetTimestamp();

                await ForeachTransaction(tableClient, addEntitiesBatch)
                    .ConfigureAwait(false);

            //PrintEtagInfo(sellOrderList, transactionResponse);

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync($"seed data batch duration: {Stopwatch.GetElapsedTime(startTime).TotalMilliseconds} ms");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            var response = req.CreateResponse();
            await response.WriteAsJsonAsync($"seed BOOM!");
            return response;

        }
    }

    private static async Task ForeachTransaction(TableClient tableClient, List<TableTransactionAction> addEntitiesBatch)
    {
        // submit batch

        foreach (var batch in addEntitiesBatch.ToTransactionBatches())
        {
            await tableClient.SubmitTransactionAsync(batch)
                .ConfigureAwait(false);
        }
    }

    private static void PrintEtagInfo(List<SellOrderEntity> sellOrderList, Response<IReadOnlyList<Response>> transactionResponse)
    {
        for (int i = 0; i < sellOrderList.Count; i++)
        {
            Console.WriteLine($"The ETag for the entity with RowKey: '{sellOrderList[i].RowKey}' is {transactionResponse.Value[i].Headers.ETag}");
        }
    }

    List<SellOrderEntity> GetFakeSellOrders(int count)
    {
        var sellOrderFaker = new Faker<SellOrderEntity>()
            .RuleFor(x => x.PartitionKey, f => partitionKey)
            .RuleFor(x => x.RowKey, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.PokemonName, f => f.PickRandom(pokemonList))
            .RuleFor(x => x.SellPrice, f => f.Commerce.Price(100, 1000, 2, "€"));

        return sellOrderFaker.Generate(count);
    }

    string[] pokemonList => [
  "bulbasaur", "ivysaur", "venusaur",
  "charmander", "charmeleon", "charizard",
  "squirtle", "wartortle", "blastoise",
  "caterpie", "metapod", "butterfree",
  "weedle", "kakuna", "beedrill",
  "pidgey", "pidgeotto", "pidgeot",
  "rattata", "raticate"
];

}

internal static class TransactionBatching
{
    internal static List<TableTransactionAction[]> ToTransactionBatches(this List<TableTransactionAction> actions) =>
        actions.Chunk(100).ToList();
};

