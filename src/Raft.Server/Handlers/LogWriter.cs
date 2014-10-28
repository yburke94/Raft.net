using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Raft.Server.Configuration;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 4 of 4 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder
    ///     LogReplicator
    ///     LogWriter*
    /// </summary>
    internal class LogWriter : CommandScheduledEventHandler
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FlushFileBuffers(SafeFileHandle hFile);

        private readonly IRaftConfiguration _raftConfiguration;

        public LogWriter(IRaftConfiguration raftConfiguration)
        {
            _raftConfiguration = raftConfiguration;
        }

        public override void Handle(CommandScheduledEvent data)
        {
            var bytes = data.Metadata["EncodedLog"] as byte[];
            if (bytes == null) throw new InvalidCastException();

            using (var file = new FileStream(_raftConfiguration.LogPath, FileMode.OpenOrCreate,
                FileAccess.ReadWrite, FileShare.None, 2 << 10, FileOptions.SequentialScan))
            {
                file.SetLength(bytes.Length); // Need to pre-allocate.
                file.Write(bytes, 0, bytes.Length);

                file.Flush();

                if (!FlushFileBuffers(file.SafeFileHandle))
                {
                    var error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(error, "An error occured whilst calling FlushFileBuffers");
                }
            }
        }
    }
}
