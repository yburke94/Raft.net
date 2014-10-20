using System.Linq;

namespace Raft.Server
{
    public interface IConfigureRaft
    {
        IRaftConfiguration Configure(RaftConfigurationBuilder builder);
    }
}
