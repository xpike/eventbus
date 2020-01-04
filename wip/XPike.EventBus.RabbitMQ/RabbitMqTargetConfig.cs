namespace XPike.EventBus.RabbitMQ
{
    public class RabbitMqTargetConfig
    {
        public string Exchange { get; set; }

        public string RoutingKey { get; set; }

        public bool Persistent { get; set; }

        public bool AutoAck { get; set; }
        
        public uint PrefetchSize { get; set; }
        
        public ushort PrefetchCount { get; set; }
        
        public bool GlobalQos { get; set; }
        
        public bool Mandatory { get; set; }

        public bool Enabled { get; set; }

        public int ConsumerChannels { get; set; }

        public bool RequeueOnFailure { get; set; }
    }
}