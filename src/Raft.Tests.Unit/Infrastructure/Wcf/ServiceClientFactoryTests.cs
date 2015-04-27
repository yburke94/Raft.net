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
    public class ServiceClientFactoryTests
    {
        [Test]
        public void CanCreateChannelGivenEndpointBindingAndContract()
        {
            // Arrange
            using (var svcHost = new TestServiceHost())
            {
                var logger = Substitute.For<ILogger>();
                var serviceClientFactory = new ServiceClientFactory<ITestService>(
                    svcHost.EndpointAddress, svcHost.EndpointBinding, logger);

                // Act
                var serviceClientProxy = serviceClientFactory.GetServiceClient();

                // Assert
                serviceClientProxy.Should().NotBeNull();
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

                var serviceClientFactory = new ServiceClientFactory<ITestService>(
                    svcHost.EndpointAddress, svcHost.EndpointBinding, logger);

                var serviceClientProxy = serviceClientFactory.GetServiceClient();

                // Act
                serviceClientProxy.DoSomething(TestServiceAction.Nothing);

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

                var serviceClientFactory = new ServiceClientFactory<ITestService>(
                    svcHost.EndpointAddress, svcHost.EndpointBinding, logger);

                var serviceClientProxy = serviceClientFactory.GetServiceClient();

                // Act
                Action actAction = () => serviceClientProxy.DoSomething(TestServiceAction.ThrowCommunicationError);

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

                var serviceClientFactory = new ServiceClientFactory<ITestService>(
                    svcHost.EndpointAddress, svcHost.EndpointBinding, logger);

                var serviceClientProxy = serviceClientFactory.GetServiceClient();
                serviceClientProxy.DoSomething(TestServiceAction.Nothing);

                // Act
                Action actAction = () => serviceClientProxy.DoSomething(TestServiceAction.Nothing);

                // Assert
                actAction.ShouldThrow<ObjectDisposedException>();
            }
        }
    }
}
