using XPike.IoC;

namespace XPike.EventBus
{
    public class Package
        : IDependencyPackage
    {
        public void RegisterPackage(IDependencyCollection dependencyCollection)
        {
            dependencyCollection.RegisterSingleton<IEventBusService, EventBusService>();
        }
    }
}