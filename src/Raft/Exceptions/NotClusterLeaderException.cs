using System;

namespace Raft.Exceptions
{
    public class NotClusterLeaderException : Exception
    {
        public override string Message
        {
            get {
                return "Cannot execute command against this server as it is not the Leader of the Raft cluster." +
                "Please use the GetClusterLeader() method on the IRaft object to get the address of the cluster leader.";
            }
        }
    }
}
