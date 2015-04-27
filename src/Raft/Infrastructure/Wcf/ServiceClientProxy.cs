using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.ServiceModel;
using Serilog;

namespace Raft.Infrastructure.Wcf
{
    internal class ServiceClientProxy<T> : RealProxy
    {
        private readonly T _clientInstance;
        private readonly ILogger _logger;

        public ServiceClientProxy(T clientInstance, ILogger logger) : base(typeof(T))
        {
            _logger = logger;
            _clientInstance = clientInstance;
        }

        public override IMessage Invoke(IMessage msg)
        {
            var methodCall = (IMethodCallMessage)msg;
            var method = (MethodInfo)methodCall.MethodBase;

            var clientInstance = (IClientChannel)_clientInstance;

            try
            {
                _logger.Debug("Calling '{operation}'.", method.Name);

                var result = method.Invoke(_clientInstance, methodCall.InArgs);
                return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
            }
            catch (TargetInvocationException tie)
            {
                var actual = tie.InnerException;

                _logger.Error(actual, "Error whilst calling '{operation}'", method.Name);

                if (clientInstance.State == CommunicationState.Faulted)
                    clientInstance.Abort();

                throw actual;
            }
            finally
            {
                if (clientInstance.State != CommunicationState.Closed)
                    clientInstance.Close();
            }
        }
    }
}