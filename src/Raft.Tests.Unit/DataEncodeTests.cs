using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ProtoBuf;
using Raft.Server;

namespace Raft.Tests.Unit
{
    [TestFixture]
    public class LogEncodeTests
    {
        [Test]
        public void CanEncodeLogMessage()
        {
            // Arrange
            var log = new LogEntry {
                Index = 1,
                Term = 1,
                CommandType = typeof (DoSomething).AssemblyQualifiedName,
                Command = new DoSomething {
                    Count = 54
                }
            };

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "ProtoEncode-" + DateTime.Now.ToFileTimeUtc() + ".bin");

            using (var file = File.Create(filePath))
            {
                // Act
                Serializer.Serialize(file, log);
            }

            // Assert
            File.Exists(filePath);
        }
    }

    [ProtoContract]
    public class DoSomething : IRaftCommand
    {
        [ProtoMember(1)]
        public int Count { get; set; }


        public void Execute(RaftServerContext context)
        {
            
        }
    }
}
