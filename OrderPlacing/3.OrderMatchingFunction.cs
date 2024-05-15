using System;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PokeTrader.Api.OrderPlacing
{
    public class OrderMatchingFunction
    {
        private readonly ILogger<OrderMatchingFunction> _logger;

        public OrderMatchingFunction(ILogger<OrderMatchingFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(OrderMatchingFunction))]
        public void Run([QueueTrigger("myqueue-items", Connection = "")] QueueMessage message)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");
        }
    }
}
