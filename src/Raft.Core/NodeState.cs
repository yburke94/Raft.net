namespace Raft.Core
{
    internal enum NodeState
    {
        Initial,
        Leader,
        Candidate,
        Follower,
        Final
    }
}