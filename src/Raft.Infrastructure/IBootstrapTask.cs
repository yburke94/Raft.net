using System.Linq;

namespace Raft.Infrastructure
{
    public interface IBootstrapTask
    {
        void Bootstrap();
    }
}
