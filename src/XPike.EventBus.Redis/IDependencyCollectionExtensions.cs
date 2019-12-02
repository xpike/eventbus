using XPike.IoC;

namespace XPike.EventBus.Redis
{
    public static class IDependencyCollectionExtensions
    {
        public static IDependencyCollection AddXPikeRedisEventBus(this IDependencyCollection collection) =>
            collection.LoadPackage(new XPike.EventBus.Redis.Package());
    }
}