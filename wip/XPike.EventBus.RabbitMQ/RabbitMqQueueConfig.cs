namespace XPike.EventBus.RabbitMQ
{
    public class RabbitMqQueueConfig
    {
        public bool Durable { get; set; }
        
        public bool AutoDelete { get; set; }
        
        public bool Exclusive { get; set; }

        public bool SkipBinding { get; set; }

        public string BindingRoutingKey { get; set; }
    }
}