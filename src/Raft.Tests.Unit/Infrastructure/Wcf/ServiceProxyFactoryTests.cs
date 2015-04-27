using System;
using System.ServiceModel;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Infrastructure.Wcf;
using Raft.Tests.Unit.TestHelpers;
using Serilog;

namespace Raft.Tests.Unit.Infrastructure.Wcf
{
    // TODO: These should be integration tests.
    [TestFixture]
    public class ServiceProxyFactoryTests
    {
        [Test]
        public void CanCreateChannelGivenEndpointBindingAndContract()
        {
            // Arrange
            using (var svcHost = new TestServiceHost())
            {
                var logger = Substitute.For<ILogger>();
                var proxyFactory = new ServiceProxyFactory<ITestService>(
                    svcHost.EndpointAddress, svcHost.EndpointBinding, logger);

                // Act
                var proxy = proxyFactory.GetProxy();

                // Assert
                proxy.Should().NotBeNull();
            }
        }

        [Test]
        public void ChannelCallLogsDebugMessageWhenCallingSvcOperation()
        {
            // Arrange
            using (var svcHost = new TestServiceHost())
            {
                svcHost.Start();

                var logger = Substitute.For<ILogger>();
                logger.ForContext(Arg.Any<string>(), Arg.Any<object>())
                    .Returns(logger);

                var proxyFactory = new ServiceProxyFactory<ITestService>(
                    svcHost.EndpointAddress, svcHost.EndpointBinding, logger);

                var proxy = proxyFactory.GetProxy();

                // Act
                proxy.DoSomething(TestServiceAction.Nothing);

                // Assert
                logger.Received(1).Debug(Arg.Any<string>(), Arg.Any<object[]>());
            }
        }

        [Test]
        public void ChannelCallLogsErrorMessageWhenCallingSvcOperationThatThrows()
        {
            // Arrange
            using (var svcHost = new TestServiceHost())
            {
                svcHost.Start();

                var logger = Substitute.For<ILogger>();
                logger.ForContext(Arg.Any<string>(), Arg.Any<object>())
                    .Returns(logger);

                var proxyFactory = new ServiceProxyFactory<ITestService>(
                    svcHost.EndpointAddress, svcHost.EndpointBinding, logger);

                var proxy = proxyFactory.GetProxy();

                // Act
                Action actAction = () => proxy.DoSomething(TestServiceAction.ThrowCommunicationError);

                // Assert
                actAction.ShouldThrow<FaultException>();
                logger.Received(1).Error(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
            }
        }

        [Test]
        public void ChannelClosesAfterCallingAnOperation()
        {
            // Arrange
            using (var svcHost = new TestServiceHost())
            {
                svcHost.Start();

                var logger = Substitute.For<ILogger>();
                logger.ForContext(Arg.Any<string>(), Arg.Any<object>())
                    .Returns(logger);

                var proxyFactory = new ServiceProxyFactory<ITestService>(
                    svcHost.EndpointAddress, svcHost.EndpointBinding, logger);

                var proxy = proxyFactory.GetProxy();
                proxy.DoSomething(TestServiceAction.Nothing);

                // Act
                Action actAction = () => proxy.DoSomething(TestServiceAction.Nothing);

                // Assert
                actAction.ShouldThrow<ObjectDisposedException>();
            }
        }
    }
}
