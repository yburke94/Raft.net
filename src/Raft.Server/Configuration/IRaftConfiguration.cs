using Raft.Infrastructure.Journaler;

namespace Raft.Server.Configuration
{
    public interface IRaftConfiguration {
        JournalConfiguration JournalConfiguration { get; set; }
    }
}