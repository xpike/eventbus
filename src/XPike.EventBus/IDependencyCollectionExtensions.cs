using XPike.IoC;

namespace XPike.EventBus
{
    public static class IDependencyCollectionExtensions
    {
        public static IDependencyCollection AddXPikeEventBus(this IDependencyCollection collection) =>
            collection.LoadPackage(new XPike.EventBus.Package());
    }
}