using System;
using XPike.IoC;

namespace XPike.EventBus
{
    public static class IDependencyProviderExtensions
    {
        public static IDependencyProvider UseXPikeEventBusProvider<TProvider>(this IDependencyProvider provider,
            string connectionName)
            where TProvider : class, IEventBusConnectionProvider
        {
            var service = provider.ResolveDependency<IEventBusService>();
            var connectionProvider = provider.ResolveDependency<TProvider>();

            if (!service.AddConnectionProvider(connectionName, connectionProvider))
                throw new InvalidOperationException($"The Event Bus connection named '{connectionName}' has already been registered with {service.GetType()}!");

            return provider;
        }

    }
}