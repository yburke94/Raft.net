using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Raft.Extensions.Journaler.Kernel
{
    internal class UnbufferedStream
    {
        // ReSharper disable InconsistentNaming
        const int FILE_FLAG_NO_BUFFERING = unchecked(0x20000000);
        const int FILE_FLAG_SEQUENTIAL_SCAN = unchecked(0x08000000);
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Creates a FileStream which will perform unbuffered writes to the disk.
        /// </summary>
        /// <remarks>
        /// BufferSize must be a multiple of the size of a single sector for the hard disk.
        /// </remarks>
        public static FileStream Get(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            var sectorSize = SectorSize.Get(path);

            if (bufferSize%sectorSize != 0)
                bufferSize = (int)sectorSize;

            var handle = CreateFile(path, (int)access, share, IntPtr.Zero, mode, FILE_FLAG_NO_BUFFERING | FILE_FLAG_SEQUENTIAL_SCAN, IntPtr.Zero);

            if (handle.IsInvalid)
                throw new IOException("Failed to create unbufferred handle.");

            return new FileStream(handle, access, bufferSize, false);
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern SafeFileHandle CreateFile(String fileName,
                                                   int desiredAccess,
                                                   FileShare shareMode,
                                                   IntPtr securityAttrs,
                                                   FileMode creationDisposition,
                                                   int flagsAndAttributes,
                                                   IntPtr templateFile);
    }
}
