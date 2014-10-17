using System.Linq;
using Automatonymous;

namespace Raft.Core
{
    public interface INodeEvents
    {
        Event JoinCluster { get; }

        Event ApplyCommand { get; }
    }
}
