# Raft.NET

[![Build status](https://ci.appveyor.com/api/projects/status/r0gwlmoak9ymqivf/branch/master?svg=true)](https://ci.appveyor.com/project/yburke94/raft-net/branch/master)

A .Net implementation of the RAFT consensus algorithm.

WORK IN PROGRESS

Technologies used:
 - Disruptor-net
 - Protocol Buffers
 - Stateless
 - WCF
 - LightInject
 - Unbuffered IO
 
Work Left:
 - Finish rules for different states
 - Finish log replication
 - Impl auto-discovery/joining of clusters
 - Make fluent configuration builders for the framework
 - Test it all and pray it works :)
