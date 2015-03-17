namespace Raft.Core.StateMachine.Enums
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