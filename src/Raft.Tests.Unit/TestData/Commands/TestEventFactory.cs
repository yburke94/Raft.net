using System;
using System.Threading.Tasks;
using Raft.Server.Events;
using Raft.Server.Events.Data;
using Raft.Server.Events.Translators;

namespace Raft.Tests.Unit.TestData.Commands
{
    internal static class TestEventFactory
    {
        public static CommandScheduled GetInternalCommandEvent()
        {
            return new CommandScheduledTranslator(
                new TestInternalCommand(),
                new TaskCompletionSource<CommandExecuted>())
                .Translate(new CommandScheduled(), 1L);
        }

        public static CommandScheduled GetCommandEvent()
        {
            return new CommandScheduledTranslator(
                new TestCommand(),
                new TaskCompletionSource<CommandExecuted>())
                .Translate(new CommandScheduled(), 1L);
        }

        public static CommandScheduled GetCommandEvent(long logIdx, byte[] data)
        {
            var @event =  new CommandScheduledTranslator(
                new TestCommand(),
                new TaskCompletionSource<CommandExecuted>())
                .Translate(new CommandScheduled(), 1L);
            @event.SetLogEntry(new LogEntry { Index = logIdx }, data);

            return @event;
        }

        public static CommandScheduled GetCommandEvent(long logIdx, byte[] data, Action executeAction)
        {
            var @event =  new CommandScheduledTranslator(
                new TestExecutableCommand(executeAction),
                new TaskCompletionSource<CommandExecuted>())
                .Translate(new CommandScheduled(), 1L);
            @event.SetLogEntry(new LogEntry { Index = logIdx }, data);
            return @event;
        }
    }
}
