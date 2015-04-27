using System;

namespace Raft.Infrastructure.Wcf
{
    internal interface IServiceClientFactory<out TService>
    {
        TService GetServiceClient();
    }
}
