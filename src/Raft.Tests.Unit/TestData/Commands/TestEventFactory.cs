using System.Linq;
using System.Threading.Tasks;
using Raft.Server;

namespace Raft.Tests.Unit.TestData.Commands
{
    internal static class TestEventFactory
    {
        public static CommandScheduledEvent GetInternalCommandEvent()
        {
            return new CommandScheduledEvent()
                .ResetEvent(new TestInternalCommand(), new TaskCompletionSource<LogResult>());
        }

        public static CommandScheduledEvent GetCommandEvent()
        {
            return new CommandScheduledEvent()
                .ResetEvent(new TestCommand(), new TaskCompletionSource<LogResult>());
        }


    }
}
