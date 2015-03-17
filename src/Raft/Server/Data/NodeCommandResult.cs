namespace Raft.Server.Data
{
    internal class NodeCommandResult
    {
        public NodeCommandResult(bool successful)
        {
            Successful = successful;
        }

        public bool Successful { get; private set; }
    }
}