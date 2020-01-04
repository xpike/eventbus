namespace XPike.EventBus.RabbitMQ
{
    public class RabbitMqExchangeConfig
    {
        public bool Durable { get; set; }
        
        public bool AutoDelete { get; set; }

        public string ExchangeType { get; set; }
    }
}