using XPike.IoC;

namespace XPike.EventBus.MassTransit.RabbitMQ
{
    public class Package
        : IDependencyPackage
    {
        public void RegisterPackage(IDependencyCollection dependencyCollection)
        {
            dependencyCollection.AddXPikeEventBus();

            dependencyCollection.RegisterSingleton<IMtxRmqEventBusConnectionProvider, MtxRmqEventBusConnectionProvider>();
            dependencyCollection.RegisterSingleton<IEventBusConnectionProvider>(services =>
                services.ResolveDependency<IMtxRmqEventBusConnectionProvider>());
        }
    }
}