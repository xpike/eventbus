using XPike.IoC;

namespace XPike.EventBus.RabbitMQ
{
    public class Package
        : IDependencyPackage
    {
        public void RegisterPackage(IDependencyCollection dependencyCollection)
        {
            dependencyCollection.AddXPikeEventBus();

            dependencyCollection.RegisterSingleton<IRabbitMqEventBusConnectionProvider, RabbitMqEventBusConnectionProvider>();
            dependencyCollection.RegisterSingleton<IEventBusConnectionProvider>(services =>
                services.ResolveDependency<IRabbitMqEventBusConnectionProvider>());
        }
    }
}