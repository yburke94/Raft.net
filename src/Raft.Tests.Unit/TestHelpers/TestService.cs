using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using System.Threading.Tasks;

namespace Raft.Tests.Unit.TestHelpers
{
    public class TestServiceHost : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public TestServiceHost() : this(
            @"net.tcp://localhost:50135/TestService",
            new NetTcpBinding(SecurityMode.None, false)) { }

        public TestServiceHost(string address, Binding binding)
        {
            EndpointAddress = address;
            EndpointBinding = binding;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public string EndpointAddress { get; private set; }

        public Binding EndpointBinding { get; private set; }

        public bool HostStarted { get; private set; }

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                using (var serviceHost = new ServiceHost(typeof(TestService), new Uri(EndpointAddress)))
                {
                    serviceHost.Description.Behaviors.Add(new ServiceMetadataBehavior {
                        MetadataExporter = { PolicyVersion = PolicyVersion.Policy15 }
                    });

                    serviceHost.AddServiceEndpoint(typeof(ITestService), EndpointBinding, string.Empty);
                    serviceHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                        MetadataExchangeBindings.CreateMexTcpBinding(), "mex");

                    serviceHost.Faulted += ServiceHost_Faulted;

                    serviceHost.Open();
                    HostStarted = true;

                    Debug.WriteLine("TestServiceHost: Service host has transitioned to 'Open' state.");
                    Debug.WriteLine("TestServiceHost: Service is ready at " + EndpointAddress);
                    
                    while (!_cancellationTokenSource.IsCancellationRequested)
                        Thread.Sleep(1000);

                    serviceHost.Close();
                    Debug.WriteLine("TestServiceHost: Service host has transitioned to 'Closed' state.");
                    HostStarted = false;
                }
            }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            if (!SpinWait.SpinUntil(() => HostStarted, 60000))
                throw new TimeoutException("Timed out waiting for service host to start.");
        }

        void ServiceHost_Faulted(object sender, EventArgs e)
        {
            Debug.WriteLine("TestServiceHost: Service host has faulted.");
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            if (_cancellationTokenSource.IsCancellationRequested) return;

            _cancellationTokenSource.Cancel();
            if (!SpinWait.SpinUntil(() => !HostStarted, 60000))
                throw new TimeoutException("Timed out waiting for service host to close.");
        }
    }

    public class TestService : ITestService
    {
        public void DoSomething(TestServiceAction action)
        {
            Console.WriteLine("Test service is executing: " + action);
            switch (action)
            {
                case TestServiceAction.ThrowCommunicationError:
                    throw new CommunicationException("Meh!");
            }
        }
    }

    [ServiceContract]
    public interface ITestService
    {
        [OperationContract]
        void DoSomething(TestServiceAction action);
    }

    public enum TestServiceAction
    {
        Nothing,
        ThrowCommunicationError
    }
}
