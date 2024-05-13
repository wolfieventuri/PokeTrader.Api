using Azure;
using Azure.Data.Tables;
using Bogus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PokeTrader.Api.SellOrders;

public class GetSellOrders
{
    private readonly ILogger<GetSellOrders> _logger;
    private readonly TableServiceClient _tableService;

    public GetSellOrders(ILogger<GetSellOrders> logger, TableServiceClient tableService)
    {
        _logger = logger;
        _tableService = tableService;
    }


    [Function("GetSellOrders")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, CancellationToken ct)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        try
        {
            var tableClient = _tableService.GetTableClient("sellorders");

            Page<SellOrderEntity>? page = tableClient
                .Query<SellOrderEntity>(filter: $"PartitionKey eq 'pokemon_sell_order'")
                .AsPages(pageSizeHint: 20)
                .FirstOrDefault();
            

            var sellOrders = new List<SellOrderEntity>();

            if (page != null)
            {
                foreach (var item in page.Values)
                {
                    sellOrders.Add(item);
                }
            }

            var jsonResponse = JsonSerializer.Serialize(sellOrders);

            var response = req.CreateResponse();
            await response.WriteStringAsync(jsonResponse);

            return response;
        }
        catch (Exception ex)
        {

            _logger.LogError(ex.Message, ex);
            var response = req.CreateResponse();
            await response.WriteStringAsync("Boom!");
            return response;

        }
    }

}
