using System.ServiceModel.Channels;
using Raft.Infrastructure.Journaler;
using Serilog;

namespace Raft.Configuration
{
    public interface IRaftConfiguration {
        JournalConfiguration JournalConfiguration { get; set; }
        Binding RaftServiceBinding { get; set; }
        ILogger Logger { get; set; }
    }
}