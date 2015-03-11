using Raft.Configuration;

namespace Raft
{
    public interface IConfigureRaft
    {
        IRaftConfiguration Configure(RaftConfigurationBuilder builder);
    }
}
