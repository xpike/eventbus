using XPike.Drivers.Declarative;

namespace XPike.Drivers.EventBus.Declarative
{
    /// <summary>
    /// A reflector class to allow for the fire-and-forget nature of most event bus implementations.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public class EventBusPublishResponse<TEvent>
        : IRespondTo<TEvent>
        where TEvent : class, IRespondWith<EventBusPublishResponse<TEvent>>
    {
    }
}