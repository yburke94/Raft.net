using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Raft.Infrastructure;

namespace Raft.Tests.Unit.Infrastructure
{
    [TestFixture]
    public class WaitableCounterTests
    {
        [Test, Timeout(2000)]
        public void WillNotBlockWhenIncrementedToLimit()
        {
            // Arrange
            const int limit = 10;
            var counter = new WaitableCounter(limit);

            // Act
            for (var i = 0; i < limit; i++)
                counter.Increment();

            // Assert
            counter.Wait();
        }

        [Test, Timeout(2000)]
        public void WillNotBlockWhenIncrementedPastLimit()
        {
            // Arrange
            const int limit = 10;
            var counter = new WaitableCounter(limit);

            // Act
            for (var i = 0; i < limit+10; i++)
                counter.Increment();

            // Assert
            counter.Wait();
        }

        [Test, Timeout(2000)]
        public void WillBlockWhenIncrementedPastLimit()
        {
            // Arrange
            const int limit = 10;
            var counter = new WaitableCounter(limit);

            for (var i = 0; i < limit - 5; i++)
                counter.Increment();

            // Act
            var task = Task.Factory.StartNew(() => counter.Wait());

            // Assert
            task.Wait(1500).Should().BeFalse();
        }
    }
}
