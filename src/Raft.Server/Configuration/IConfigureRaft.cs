using System.Linq;

namespace Raft.Server.Configuration
{
    public interface IConfigureRaft
    {
        IRaftConfiguration Configure(RaftConfigurationBuilder builder);
    }
}
