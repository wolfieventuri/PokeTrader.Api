using Azure.Data.Tables;
using Bogus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace PokeTrader.Api.SellOrders;

public class SeedSellOrdersForEach
{
    private readonly ILogger<GetSellOrders> _logger;
    private readonly TableServiceClient _tableService;

    public SeedSellOrdersForEach(ILogger<GetSellOrders> logger, TableServiceClient tableService)
    {
        _logger = logger;
        _tableService = tableService;
    }

    [Function("SeedPokemonSellOrdersForEach")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, CancellationToken ct)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        try
        {
            var startTime = Stopwatch.GetTimestamp();
            var tableClient = _tableService.GetTableClient("pokemonsellorders");

            var sellOrderList = GetFakeSellOrders(1000);

            foreach (var pokemon in sellOrderList)
            {
                await tableClient.AddEntityAsync(pokemon, ct);
            }

            var response = req.CreateResponse();
            await response.WriteStringAsync($"seed data batch duration: {Stopwatch.GetElapsedTime(startTime).TotalMilliseconds} ms");
            return response;
        }
        catch (Exception ex)
        {

            _logger.LogError(ex.Message, ex);
            var response = req.CreateResponse();
            response.WriteString("Boom!");
            return response;

        }
    }



    List<SellOrderEntity> GetFakeSellOrders(int count)
    {
        var sellOrderFaker = new Faker<SellOrderEntity>()
            .RuleFor(x => x.PartitionKey, f => "pokemon_sell_order")
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
