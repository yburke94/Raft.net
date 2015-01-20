using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;

namespace Raft.Infrastructure.Journaler.Kernel
{
    internal class SectorSize
    {
        private static readonly ConcurrentDictionary<string, uint> DriveSectorSizeMap = new ConcurrentDictionary<string, uint>();

        /// <summary>
        /// Return the sector size of the volume for the specified file path.
        /// </summary>
        /// <returns>Device sector size in bytes </returns>
        public static uint Get(string uncPath)
        {
            uint size = 0;

            var drive = Path.GetPathRoot(uncPath);
            if (drive == null) throw new ArgumentException(
                "Could not retrieve drive from specified UNC path. Please ensure a valid path is specified.");

            if (DriveSectorSizeMap.TryGetValue(drive, out size))
                return size;

            // ignored outputs
            uint ignore;

            GetDiskFreeSpace(Path.GetPathRoot(uncPath), out ignore, out size, out ignore, out ignore);
            DriveSectorSizeMap.AddOrUpdate(drive, size, (d, s) => size);

            return size;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern bool GetDiskFreeSpace(string path,
                                            out uint sectorsPerCluster,
                                            out uint bytesPerSector,
                                            out uint numberOfFreeClusters,
                                            out uint totalNumberOfClusters);
    }
}