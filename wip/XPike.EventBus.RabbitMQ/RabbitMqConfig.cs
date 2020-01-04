using System.Collections.Generic;

namespace XPike.EventBus.RabbitMQ
{
    public class RabbitMqConfig
    {
        public Dictionary<string, RabbitMqConnectionConfig> Connections { get; set; }
    }
}