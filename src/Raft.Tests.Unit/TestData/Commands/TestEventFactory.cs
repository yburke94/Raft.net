using System;
using System.Threading.Tasks;
using Raft.Server;

namespace Raft.Tests.Unit.TestData.Commands
{
    internal static class TestEventFactory
    {
        public static CommandScheduledEvent GetInternalCommandEvent()
        {
            return new CommandScheduledEvent()
                .ResetEvent(new TestInternalCommand(), new TaskCompletionSource<CommandExecutionResult>());
        }

        public static CommandScheduledEvent GetCommandEvent()
        {
            return new CommandScheduledEvent()
                .ResetEvent(new TestCommand(), new TaskCompletionSource<CommandExecutionResult>());
        }

        public static CommandScheduledEvent GetCommandEvent(Action executeAction)
        {
            return new CommandScheduledEvent()
                .ResetEvent(new TestExecutableCommand(executeAction), new TaskCompletionSource<CommandExecutionResult>());
        }


    }
}
