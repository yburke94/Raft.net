using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raft.Server
{
    internal interface INodeTimer
    {
        void ResetTimer();
    }
}
