namespace Raft.Infrastructure.Wcf
{
    internal interface IServiceProxyFactory<out TService>
    {
        TService GetProxy();
    }
}
