using System;
using Newtonsoft.Json;
using XPike.EventBus.RabbitMQ;
using Xunit;
using Xunit.Abstractions;

namespace XPike.EventBus.UnitTests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Test1()
        {
            const string json =
                "{\r\n          \"Connections\": {\r\n            \"heartbeat\": {\r\n              \"Hostname\": \"127.0.0.1\",\r\n              \"Port\": 5673,\r\n              \"VirtualHost\": \"\",\r\n              \"Username\": \"guest\",\r\n              \"Password\": \"guest\",\r\n              \"PurgeOnStartup\": false,\r\n              \"ClusterMembers\": [\"172.20.0.16\", \"172.20.0.13\", \"172.20.0.9\"],\r\n              \"PublisherConfirmation\": true,\r\n              \"SslProtocols\": [ \"Tls12\" ],\r\n              \"Targets\": {\r\n                \"heartbeatPublish\": {\r\n                  \"Exchange\": \"heartbeat\",\r\n                  \"RoutingKey\": \"\",\r\n                  \"Persistent\": true,\r\n                  \"AutoAck\": false,\r\n                  \"PrefetchSize\": 0,\r\n                  \"PrefetchCount\": 1,\r\n                  \"GlobalQos\": false,\r\n                  \"Mandatory\": true,\r\n                  \"Enabled\": true,\r\n                  \"ConsumerChannels\": 2,\r\n                  \"RequeueOnFailure\": true\r\n                },\r\n                \"heartbeatConsume\": {\r\n                  \"Exchange\": \"\",\r\n                  \"RoutingKey\": \"heartbeat\",\r\n                  \"Persistent\": true,\r\n                  \"AutoAck\": false,\r\n                  \"PrefetchSize\": 0,\r\n                  \"PrefetchCount\": 1,\r\n                  \"GlobalQos\": false,\r\n                  \"Mandatory\": true,\r\n                  \"Enabled\": true,\r\n                  \"ConsumerChannels\": 2,\r\n                  \"RequeueOnFailure\": true\r\n                }\r\n              },\r\n              \"Exchanges\": {\r\n                \"heartbeat\": {\r\n                  \"Durable\": true,\r\n                  \"AutoDelete\": false,\r\n                  \"ExchangeType\": \"Fanout\"\r\n                } \r\n              },\r\n              \"Queues\": {\r\n                \"heartbeat\": {\r\n                  \"Durable\": true,\r\n                  \"AutoDelete\": false,\r\n                  \"Exclusive\": false,\r\n                  \"SkipBinding\": false,\r\n                  \"BindingRoutingKey\": \"\" \r\n                } \r\n              },\r\n              \"Enabled\": true,\r\n              \"MaxPublisherChannels\": 4 \r\n            } \r\n          } \r\n        }";
            var config = JsonConvert.DeserializeObject<RabbitMqConfig>(json);
            _testOutputHelper.WriteLine(JsonConvert.SerializeObject(config,
                                                          Formatting.None)
                                         .Replace("\"",
                                                  "'"));
        }
    }
}
