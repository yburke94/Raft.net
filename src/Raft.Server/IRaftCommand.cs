using Microsoft.Practices.ServiceLocation;

namespace Raft.Server
{
    public interface IRaftCommand
    {
        void Execute(IServiceLocator serviceLocator);
    }
}