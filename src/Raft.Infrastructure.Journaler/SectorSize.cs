using System.Runtime.InteropServices;

namespace Raft.Infrastructure.Journaler
{
    internal class SectorSize
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern bool GetDiskFreeSpace(string path,
                                            out uint sectorsPerCluster,
                                            out uint bytesPerSector,
                                            out uint numberOfFreeClusters,
                                            out uint totalNumberOfClusters);

        /// <summary>
        /// Return the sector size of the volume the specified filepath lives on.
        /// </summary>
        /// <returns>device sector size in bytes </returns>
        public static uint Get(string drive)
        {
            uint size = 0;

            // ignored outputs
            uint ignore;

            GetDiskFreeSpace(drive, out ignore, out size, out ignore, out ignore);
            return size;
        }
    }
}