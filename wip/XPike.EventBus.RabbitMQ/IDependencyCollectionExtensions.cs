using XPike.IoC;

namespace XPike.EventBus.RabbitMQ
{
    public static class IDependencyCollectionExtensions
    {
        public static IDependencyCollection AddXPikeRabbitMqEventBus(this IDependencyCollection collection) =>
            collection.LoadPackage(new XPike.EventBus.RabbitMQ.Package());
    }
}