using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Raft.Infrastructure.Journaler
{
    public class UnbufferedStream
    {
        const int FILE_FLAG_NO_BUFFERING = unchecked(0x20000000);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern SafeFileHandle CreateFile(String fileName,
                                                   int desiredAccess,
                                                   FileShare shareMode,
                                                   IntPtr securityAttrs,
                                                   FileMode creationDisposition,
                                                   int flagsAndAttributes,
                                                   IntPtr templateFile);

        /// <remarks>
        /// bufferSize must be a multiple of the size of a single sector for the hard disk.
        /// </remarks>
        public static FileStream Get(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            var handle = CreateFile(path, (int)access, share, IntPtr.Zero, mode, FILE_FLAG_NO_BUFFERING, IntPtr.Zero);

            if (handle.IsInvalid)
                throw new IOException("Failed to create unbufferred handle.");

            return new FileStream(handle, access, bufferSize, false);
        }
    }
}
