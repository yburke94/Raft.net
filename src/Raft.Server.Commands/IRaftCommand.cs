using Microsoft.Practices.ServiceLocation;

namespace Raft.Server.Commands
{
    public interface IRaftCommand
    {
        void Execute(IServiceLocator serviceLocator);
    }
}