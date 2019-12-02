namespace XPike.EventBus
{
    public enum PublicationType
    {
        Unknown = 0,

        /// <summary>
        /// Represents a simple pub-sub model where each event will be received by all listeners.
        /// Examples would be Redis pub/sub, a Kinesis stream, or an Azure Service Bus Topic.
        /// </summary>
        BroadcastEvent = 1,

        /// <summary>
        /// Represents a competing consumer queue model where each event should be received and processed by only one listener.
        /// Examples would be an RabbitMQ or an Azure Service Bus Queue.
        /// </summary>
        EnqueueCommand = 2
    }
}