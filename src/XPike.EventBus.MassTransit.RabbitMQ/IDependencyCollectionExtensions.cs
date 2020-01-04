using XPike.IoC;

namespace XPike.EventBus.MassTransit.RabbitMQ
{
    public static class IDependencyCollectionExtensions
    {
        public static IDependencyCollection AddXPikeMassTransitRabbitMqEventBus(this IDependencyCollection collection) =>
            collection.LoadPackage(new Package());
    }
}