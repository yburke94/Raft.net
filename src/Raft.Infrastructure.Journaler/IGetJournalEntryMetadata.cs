using System.IO;

namespace Raft.Infrastructure.Journaler
{
    interface IGetJournalEntryMetadata
    {
        EntryMetadata GetMetadata(FileStream stream);
    }
}
