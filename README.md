# Raft.NET

[![Build status](https://ci.appveyor.com/api/projects/status/r0gwlmoak9ymqivf/branch/master?svg=true)](https://ci.appveyor.com/project/yburke94/raft-net/branch/master)

A .Net implementation of the RAFT consensus algorithm. This is still WORK IN PROGRESS.

### High Level Design
This framework makes heavy use of ring buffers to facilitate message passing between threads. When a command is executed against the cluster leader, it is placed in the leader ring buffer. This buffer has 4 consumer threads that perform the following for each entry in the ring buffer (in order):
 - Encode the log entry (done using ProtBuf).
 - Writing the log entry to disk (using the journaler).
 - Replicating the log entry to peer nodes.
 - Updating the internal state of the node.
 
#### Replication
For each node in the cluster, there is an Actor that is responsible for replicating new entries as well as ensuring the log for that node remains consistent with the leader. The Replication buffer handler(for the leader ring buffer) is responsible for: cloning the entry for each node, posting the new entry to the corresponding Actors, and waiting until at least half of the actors have successfully replicated the log entry.

Followers that receive the AppendEntries request ensure the entry is valid (based on rules outlined in the whitepaper) and publish the new entry (along with other information received in the request) to the follower ring buffer. This buffer has 3 consumer threads that perform the following for each entry in the buffer (in order):
 - Truncate log if log matching (followers log is inconsistent).
 - Write entries to disk (using the journaler).
 - Apply entries that have been successfully commited by the leader.

#### Persistance
All encoded entries are written to journal files. The journal files are append-only and their size is pre-allocated to ensure reading and writing to the file can be done sequentially. The journaller by default uses Unbuffered IO and ensures the blocks written to the journal files are padded to align to hard drive sector boundaries. Additionally, the journaller will allow you to pass metadata for the block which will be encoded and written next to the journal entry.

##### Things left to do (there is quite a bit):
 - Compress entries in memory. Entries are kept in memory for log matching when replicating. In the solution there is a ZipList structure that will encode entries in a contiguous block of memory. This structure should be used to hold entries for each term (currently an array is used). Once the term ends, use a compression lib (such as Snappy) to compress the block in memory.
 - Implement rules around elections and voting!!!!
 - Implement auto-discovery/joining of nodes to the cluster. This will be done by utilising multicast IP so the nodes can gossip about which other nodes are in the cluster. This will also be used for heartbeats from the leader to the followers.
 - Make fluent configuration builders for the framework to make installing into the applciation easy.
 - Create test application and test, test, test!!!

#####Technologies used:
 - Disruptor-net
 - TPL Dataflow
 - Protocol Buffers
 - Stateless
 - WCF
 - LightInject
