using Microsoft.Practices.ServiceLocation;

namespace Raft.Contracts
{
    public interface IRaftCommand
    {
        void Execute(IServiceLocator serviceLocator);
    }
}