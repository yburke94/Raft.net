namespace Raft.Core.StateMachine.Enums
{
    // TODO: Become objects!!!
    public enum NodeState
    {
        Initial,
        Leader,
        Candidate,
        Follower,
        Final
    }
}