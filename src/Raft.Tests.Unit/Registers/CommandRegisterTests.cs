using System;
using FluentAssertions;
using NUnit.Framework;
using Raft.Core.Events;
using Raft.Server.Registers;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit.Registers
{
    [TestFixture]
    public class CommandRegisterTests
    {
        [Test]
        public void RemovesCommandsForOlderTermsWhenTermChanges()
        {
            // Arrange
            const long term = 1L;
            const long logIdx = 3L;
            var command = new TestCommand();

            var commandRegister = new CommandRegister();
            commandRegister.Add(term, logIdx, command);
            commandRegister.Get(term, logIdx).Should().Be(command);

            // Act
            commandRegister.Handle(new TermChanged(term+1));

            // Assert
            commandRegister.Get(term, logIdx).Should().BeNull();
        }

        [Test]
        public void DoesNotRemoveCommandsForTermsMatchingNewTermWhenTermChanges()
        {
            // Arrange
            const long term = 2L;
            const long logIdx = 3L;
            var command = new TestCommand();

            var commandRegister = new CommandRegister();
            commandRegister.Add(term, logIdx, command);
            commandRegister.Get(term, logIdx).Should().Be(command);

            // Act
            commandRegister.Handle(new TermChanged(term));

            // Assert
            commandRegister.Get(term, logIdx).Should().Be(command);
        }
    }
}
