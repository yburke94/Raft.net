namespace Raft.Core.Enums
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