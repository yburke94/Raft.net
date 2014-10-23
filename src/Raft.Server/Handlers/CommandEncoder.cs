using System.IO;
using System.Linq;
using ProtoBuf;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 2 of 4 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     CommandEncoder*
    ///     LogReplicator
    ///     LogPersistor
    /// </summary>
    internal class CommandEncoder : CommandScheduledEventHandler
    {
        public CommandEncoder() : base(skipInternalCommands:true) { }

        public override void Handle(CommandScheduledEvent data)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, data.Command);
                data.Metadata["EncodedCommand"] = ms.ToArray();
            }
        }
    }
}
