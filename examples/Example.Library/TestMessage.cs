using ProtoBuf;
using System;
using System.Runtime.Serialization;

namespace Example.Library
{
    [Serializable]
    [DataContract]
    [ProtoContract]
    public class TestMessage
    {
        [DataMember]
        [ProtoMember(1)]
        public string Source { get; set; }

        [DataMember]
        [ProtoMember(2)]
        public string Message { get; set; }

        [DataMember]
        [ProtoMember(3)]
        public Guid MessageId { get; set; }

        [DataMember]
        [ProtoMember(4)]
        public DateTime Created { get; set; }
    }
}