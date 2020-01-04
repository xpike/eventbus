using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Example.Library
{
    [Serializable]
    [DataContract]
    [ProtoContract]
    public class HeartbeatMessage
    {
        [DataMember]
        [ProtoMember(1)]
        public DateTime Timestamp { get; set; }

        [DataMember]
        [ProtoMember(2)]
        public string Origin { get; set; }
    }
}