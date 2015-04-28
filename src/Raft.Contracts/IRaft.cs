using System.Threading.Tasks;

namespace Raft.Contracts
{
    public interface IRaft
    {
        Task ExecuteCommand<T>(T command) where T : IRaftCommand, new();

        string GetClusterLeader();
    }
}