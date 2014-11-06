using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Raft.Server.Configuration;
using Raft.Server.Messages.AppendEntries;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 1 of 4 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder
    ///     LogReplicator*
    ///     LogWriter
    /// </summary>
    internal class LogReplicator : CommandScheduledEventHandler
    {
        private readonly IRaftCluster _cluster;

        public LogReplicator(IRaftCluster cluster)
        {
            _cluster = cluster;
        }

        public override void Handle(CommandScheduledEvent data)
        {
            var successfullyReplicated = 0;
            var nodesCount = _cluster.Nodes.Count;
            var nodeResponses = PostToAllNodes(_cluster.Nodes).ToList();

            while (nodeResponses.Any() && successfullyReplicated <= nodesCount/2)
            {
                var taskIdx = Task.WaitAny(nodeResponses.Cast<Task>().ToArray());
                var task = nodeResponses[taskIdx];

                // if (task.Result.Content...Was Successfull) successfullyReplicated++;
                // else retry(task.Headers.Node)?

                nodeResponses.RemoveAt(taskIdx);
            }

            // Handle visibily on what happens with remaining tasks e.g. ...ContinueWith(x => x.Log());
        }

        public static IEnumerable<Task<HttpResponseMessage>> PostToAllNodes(IList<ClusterNode> nodes)
        {
            foreach (var clusterNode in nodes)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = clusterNode.BaseAddress;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    yield return client.PostAsJsonAsync(@"api/AppendEntries", new AppendEntries());
                }
            }
        }
    }

    internal interface IRaftCluster
    {
        IList<ClusterNode> Nodes { get; set; }
    }

    internal class ClusterNode
    {
        public Uri BaseAddress { get; set; }
    }
}
