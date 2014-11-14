﻿using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Raft.Infrastructure.IO
{
    internal static class FileStreamExtensions
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FlushFileBuffers(SafeFileHandle hFile);

        public static void FlushProperly(this FileStream fs)
        {
            fs.Flush();
            if (FlushFileBuffers(fs.SafeFileHandle))
                return;

            var error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error, "An error occured whilst calling FlushFileBuffers");
        }
    }
}
