using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using NSubstitute;
using NUnit.Framework;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Infrastructure.Disruptor;
using Raft.Server;
using Raft.Server.BufferEvents;
using Raft.Server.Data;
using Raft.Server.Handlers.Follower;
using Raft.Tests.Unit.TestData.Commands;
using Raft.Tests.Unit.TestHelpers;

namespace Raft.Tests.Unit.Server.Handlers.Follower
{
    [TestFixture]
    public class RpcCommandApplierTests
    {
        [Test]
        public void AppliesRequestEntriesIfLogIdxIsLessThanOrEqualToLeaderCommit()
        {
            // Arrange
            var executed = 0;

            var request = new AppendEntriesRequested
            {
                LeaderCommit = 2L,
                EntriesDeserialized = new []
                {
                    new LogEntry {
                        Index = 1L,
                        Command = new TestExecutableCommand(() => executed++)
                    },
                    new LogEntry {
                        Index = 2L,
                        Command = new TestExecutableCommand(() => executed++)
                    }
                }
            };

            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties { LastApplied = request.LeaderCommit.Value });

            var commandRegister = new CommandRegister();
            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();
            var serviceLocator = Substitute.For<IServiceLocator>();
            var handler = new RpcCommandApplier(serviceLocator, node, commandRegister, nodePublisher);

            // Act
            handler.OnNext(request, 0L, true);

            // Assert
            executed.Should().Be(2);
        }

        [Test]
        public void DoesntApplyRequestEntriesIfLogIdxIsGreaterThanLeaderCommit()
        {
            // Arrange
            var executed = 0;

            var request = new AppendEntriesRequested
            {
                LeaderCommit = 2L,
                EntriesDeserialized = new[]
                {
                    new LogEntry {
                        Index = 3L,
                        Command = new TestExecutableCommand(() => executed++)
                    },
                    new LogEntry {
                        Index = 4L,
                        Command = new TestExecutableCommand(() => executed++)
                    }
                }
            };

            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties { LastApplied = request.LeaderCommit.Value });

            var commandRegister = new CommandRegister();
            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();
            var serviceLocator = Substitute.For<IServiceLocator>();
            var handler = new RpcCommandApplier(serviceLocator, node, commandRegister, nodePublisher);

            // Act
            handler.OnNext(request, 0L, true);

            // Assert
            executed.Should().Be(0);
        }

        [Test]
        public void ThrowIfRequestEntriesIndexesAreNotSequential()
        {
            // Arrange
            var request = new AppendEntriesRequested
            {
                LeaderCommit = 6L,
                EntriesDeserialized = new[]
                {
                    new LogEntry {
                        Index = 3L,
                        Command = new TestCommand()
                    },
                    new LogEntry {
                        Index = 5L,
                        Command = new TestCommand()
                    }
                }
            };

            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties { LastApplied = request.LeaderCommit.Value });

            var commandRegister = new CommandRegister();
            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();
            var serviceLocator = Substitute.For<IServiceLocator>();
            var handler = new RpcCommandApplier(serviceLocator, node, commandRegister, nodePublisher);

            // Act
            var actAction = new Action(() => handler.OnNext(request, 0L, true));

            // Assert
            actAction.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void FireCommandAppliedNodeEventForEachValidEntryOnRequest()
        {
            // Arrange
            var request = new AppendEntriesRequested
            {
                LeaderCommit = 2L,
                EntriesDeserialized = new[]
                {
                    new LogEntry {
                        Index = 1L,
                        Command = new TestCommand()
                    },
                    new LogEntry {
                        Index = 2L,
                        Command = new TestCommand()
                    }
                }
            };

            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties { LastApplied = request.LeaderCommit.Value });

            var commandRegister = new CommandRegister();
            var nodePublisher = new TestBufferPublisher<InternalCommandScheduled>();
            var serviceLocator = Substitute.For<IServiceLocator>();
            var handler = new RpcCommandApplier(serviceLocator, node, commandRegister, nodePublisher);

            // Act
            handler.OnNext(request, 0L, true);

            // Assert
            nodePublisher.Events.Count.Should().Be(2);
            var startIdx = 0;
            nodePublisher.Events.ToList().ForEach(ev =>
            {
                ev.Command.Should().BeOfType<ApplyEntry>();
                ((ApplyEntry)ev.Command).EntryIdx.Should().Be(++startIdx);
            });
        }

        [Test]
        public void EntriesOnRequestWithIdxGreaterThanLeaderCommitAreAddedToCommandRegisterAndAreNotExecuted()
        {
            // Arrange
            var executed = 0;

            var request = new AppendEntriesRequested
            {
                LeaderCommit = 3L,
                EntriesDeserialized = new[]
                {
                    new LogEntry {
                        Index = 3L,
                        Command = new TestExecutableCommand(() => executed++)
                    },
                    new LogEntry {

                        Index = 4L,
                        Command = new TestExecutableCommand(() => executed++)
                    },
                    new LogEntry {
                        Index = 5L,
                        Command = new TestExecutableCommand(() => executed++)
                    }
                }
            };

            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties { CurrentTerm = 3L, LastApplied = request.LeaderCommit.Value });

            var commandRegister = new CommandRegister();
            var nodePublisher = new TestBufferPublisher<InternalCommandScheduled>();
            var serviceLocator = Substitute.For<IServiceLocator>();
            var handler = new RpcCommandApplier(serviceLocator, node, commandRegister, nodePublisher);

            // Act
            handler.OnNext(request, 0L, true);

            // Assert
            executed.Should().Be(1);
            commandRegister.Get(3L, 4L).Should().NotBeNull(); // Term=3(Current Term); Idx=4
            commandRegister.Get(3L, 5L).Should().NotBeNull(); // Term=3(Current Term); Idx=5
        }

        [Test]
        public void ExecuteCommandsFromRegisterWhenLastAppliedLessThanLeaderCommitAndEntriesFormRequestAreExecuted()
        {
            // Arrange
            const long term = 3L;

            var executedFromRequest = 0;
            var executedFromRegister = 0;

            var request = new AppendEntriesRequested
            {
                LeaderCommit = 4L,
                EntriesDeserialized = new[]
                {
                    new LogEntry {
                        Index = 1L,
                        Command = new TestExecutableCommand(() => executedFromRequest++)
                    },
                    new LogEntry {

                        Index = 2L,
                        Command = new TestExecutableCommand(() => executedFromRequest++)
                    }
                }
            };

            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties { CurrentTerm = term });

            var commandRegister = new CommandRegister();
            commandRegister.Add(term, 3L, new TestExecutableCommand(() => executedFromRegister++));
            commandRegister.Add(term, 4L, new TestExecutableCommand(() => executedFromRegister++));

            var nodePublisher = new TestBufferPublisher<InternalCommandScheduled>();
            nodePublisher.OnPublish(() => node.Properties.LastApplied++, false);

            var serviceLocator = Substitute.For<IServiceLocator>();
            var handler = new RpcCommandApplier(serviceLocator, node, commandRegister, nodePublisher);

            // Act
            handler.OnNext(request, 0L, true);

            // Assert
            executedFromRequest.Should().Be(2);
            executedFromRegister.Should().Be(2);
        }

        [Test]
        public void ExecuteCommandsFromRegisterWhenLastAppliedLessThanLeaderCommitAndEntriesFromRequest()
        {
            // Arrange
            const long term = 3L;

            var executedFromRegister = 0;

            var request = new AppendEntriesRequested
            {
                LeaderCommit = 4L,
                EntriesDeserialized = null
            };

            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties { CurrentTerm = term, LastApplied = 2L });

            var commandRegister = new CommandRegister();
            commandRegister.Add(term, 3L, new TestExecutableCommand(() => executedFromRegister++));
            commandRegister.Add(term, 4L, new TestExecutableCommand(() => executedFromRegister++));

            var nodePublisher = new TestBufferPublisher<InternalCommandScheduled>();
            var serviceLocator = Substitute.For<IServiceLocator>();
            var handler = new RpcCommandApplier(serviceLocator, node, commandRegister, nodePublisher);

            // Act
            handler.OnNext(request, 0L, true);

            // Assert
            executedFromRegister.Should().Be(2);
        }

        [Test]
        public void FiresApplyEntryToNodeWhenExecutingCommandsFromRegister()
        {
            // Arrange
            const long term = 3L;

            var request = new AppendEntriesRequested
            {
                LeaderCommit = 4L,
                EntriesDeserialized = null
            };

            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties { CurrentTerm = term, LastApplied = 2L });

            var commandRegister = new CommandRegister();
            commandRegister.Add(term, 3L, new TestCommand());
            commandRegister.Add(term, 4L, new TestCommand());

            var nodePublisher = new TestBufferPublisher<InternalCommandScheduled>();
            var serviceLocator = Substitute.For<IServiceLocator>();
            var handler = new RpcCommandApplier(serviceLocator, node, commandRegister, nodePublisher);

            // Act
            handler.OnNext(request, 0L, true);

            // Assert
            nodePublisher.Events.Count.Should().Be(2);
            var startIdx = 2;
            nodePublisher.Events.ToList().ForEach(ev =>
            {
                ev.Command.Should().BeOfType<ApplyEntry>();
                ((ApplyEntry)ev.Command).EntryIdx.Should().Be(++startIdx);
            });
        }

        [Test]
        public void DoesCompleteTaskSuccessfullyInTaskCompletionSourceAfterApplyingEntries()
        {
            // Arrange
            const long term = 3L;

            var request = new AppendEntriesRequested
            {
                LeaderCommit = 4L,
                EntriesDeserialized = null
            };

            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties { CurrentTerm = term, LastApplied = 2L });

            var commandRegister = new CommandRegister();
            commandRegister.Add(term, 3L, new TestCommand());
            commandRegister.Add(term, 4L, new TestCommand());

            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();
            var serviceLocator = Substitute.For<IServiceLocator>();
            var handler = new RpcCommandApplier(serviceLocator, node, commandRegister, nodePublisher);

            // Act
            handler.OnNext(request, 0L, true);

            // Assert
            request.HasCompletedSuccessfully().Should().BeTrue();
        }

        [Test]
        public void ThrowsWhenCommandToExecuteDidNotExistInRegister()
        {
            // Arrange
            const long term = 3L;

            var request = new AppendEntriesRequested
            {
                LeaderCommit = 5L,
                EntriesDeserialized = null
            };

            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties { CurrentTerm = term, LastApplied = 2L });

            var commandRegister = new CommandRegister();
            commandRegister.Add(term, 3L, new TestCommand());
            commandRegister.Add(term, 5L, new TestCommand());

            var nodePublisher = new TestBufferPublisher<InternalCommandScheduled>();
            var serviceLocator = Substitute.For<IServiceLocator>();
            var handler = new RpcCommandApplier(serviceLocator, node, commandRegister, nodePublisher);

            // Act
            var actAction = new Action(() => handler.OnNext(request, 0L, true));

            // Assert
            actAction.ShouldThrow<InvalidOperationException>();
        }
    }
}
