namespace Raft.Core.StateMachine.Enums
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