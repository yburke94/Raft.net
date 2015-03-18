using System;
using System.Runtime.Serialization;

namespace Raft.Service.Contracts
{
    [DataContract]
    public class MultipleLeadersForTermFault
    {
        [DataMember]
        public Guid Id { get; set; }
    }
}
