using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using NUnit.Framework;
using Raft.Infrastructure;

namespace Raft.Tests.Unit.Infrastructure
{
    [TestFixture]
    public class ActorTests
    {
        [Test]
        public void CanLinkToSourceAndHandleMessages()
        {
            // Arrange
            const string message = "HelloWorld";

            var source = new BufferBlock<string>();
            var actor = new TestActor();

            // Act
            actor.AddSourceLink(source);
            source.Post(message);

            source.Complete();
            source.Completion.Wait();

            actor.Finish();

            // Assert
            actor.LastMessage.Should().Be(message);
        }

        private class TestActor : Actor<string>
        {
            public string LastMessage = string.Empty;

            public override void Handle(string message)
            {
                LastMessage = message;
            }

            public void Finish()
            {
                CompleteActor();
            }
        }
    }
}
