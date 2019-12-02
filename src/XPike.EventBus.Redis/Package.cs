using XPike.IoC;
using XPike.Redis;

namespace XPike.EventBus.Redis
{
    public class Package
        : IDependencyPackage
    {
        public void RegisterPackage(IDependencyCollection dependencyCollection)
        {
            dependencyCollection.AddXPikeRedis();
            dependencyCollection.AddXPikeEventBus();

            dependencyCollection.RegisterSingleton<IRedisEventBusConnectionProvider, RedisEventBusConnectionProvider>();
            dependencyCollection.RegisterSingleton<IEventBusConnectionProvider>(services =>
                services.ResolveDependency<IRedisEventBusConnectionProvider>());
        }
    }
}