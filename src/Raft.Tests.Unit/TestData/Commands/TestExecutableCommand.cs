using System;
using Microsoft.Practices.ServiceLocation;
using ProtoBuf;
using Raft.Server;
using Raft.Server.Commands;

namespace Raft.Tests.Unit.TestData.Commands
{
    [ProtoContract(SkipConstructor = true)]
    public class TestExecutableCommand : IRaftCommand
    {
        private readonly Action _action;

        [ProtoMember(1)]
        public int SomeSortOfState { get; set; }

        public TestExecutableCommand(Action action)
        {
            _action = action;
        }

        public void Execute(IServiceLocator serviceLocator)
        {
            _action();
        }
    }
}