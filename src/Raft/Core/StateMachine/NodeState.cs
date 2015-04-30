namespace Raft.Core.StateMachine
{
    public enum NodeState
    {
        Initial,
        Leader,
        Candidate,
        Follower,
        Final
    }
}