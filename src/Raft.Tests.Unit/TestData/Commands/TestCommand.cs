using Microsoft.Practices.ServiceLocation;
using ProtoBuf;
using Raft.Contracts;
using Raft.Server;

namespace Raft.Tests.Unit.TestData.Commands
{
    [ProtoContract(SkipConstructor = true)]
    public class TestCommand : IRaftCommand
    {
        public TestCommand()
        {
            Count = int.MaxValue;
        }

        [ProtoMember(1)]
        public int Count { get; set; }

        public void Execute(IServiceLocator serviceLocator)
        {
            // Do Nothing!
        }
    }
}
