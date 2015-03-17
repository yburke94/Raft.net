namespace Raft.Server.Data
{
    public class NodeCommandResult
    {
        public NodeCommandResult(bool successful)
        {
            Successful = successful;
        }

        public bool Successful { get; private set; }
    }
}