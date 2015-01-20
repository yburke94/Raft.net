using ProtoBuf;
using Raft.Server;
using Raft.Server.Commands;

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

        public void Execute(RaftServerContext context)
        {
            // Do Nothing!
        }
    }
}
