namespace XPike.EventBus.Redis
{
    public interface IRedisEventBusConnection
        : IRedisEventBusPublisherConnection,
          IRedisEventBusSubscriberConnection
    {
    }
}