namespace PokeTrader.Api;

public static class StorageConfiguration
{
    public const string SellOrderTableName = "sellorders";
    public const string BuyOrderTableName = "buyorders";
    public const string PlaceBuyOrderQueueName = "place-buy-order";
    public const string OutboxQueueName = "outbox-bus";
}