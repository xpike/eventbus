using System.Collections.Generic;

namespace XPike.EventBus.RabbitMQ
{
    public class RabbitMqConnectionConfig
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string VirtualHost { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool PurgeOnStartup { get; set; }
        public string ClusterMembers { get; set; }
        public bool PublisherConfirmation { get; set; }
        public string SslProtocols { get; set; }
        public Dictionary<string, RabbitMqTargetConfig> Targets { get; set; }
        public Dictionary<string, RabbitMqExchangeConfig> Exchanges { get; set; }
        public Dictionary<string, RabbitMqQueueConfig> Queues { get; set; }
        public bool Enabled { get; set; }
        public int MaxPublisherChannels { get; set; }
    }
}