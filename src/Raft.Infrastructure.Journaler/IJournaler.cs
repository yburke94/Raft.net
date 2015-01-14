using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raft.Infrastructure.Journaler
{
    interface IJournaler
    {
        void WriteBlock(byte[] block);

        void WriteBlocks(byte[][] blocks);
    }
}
