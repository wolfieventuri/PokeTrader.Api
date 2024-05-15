using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PokeTrader.Api.OrderPlacing
{
    public class PlaceOrderHttpTrigger
    {
        private readonly ILogger<PlaceOrderHttpTrigger> _logger;

        public PlaceOrderHttpTrigger(ILogger<PlaceOrderHttpTrigger> logger)
        {
            _logger = logger;
        }

        [Function("PlaceOrderHttpTrigger")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
