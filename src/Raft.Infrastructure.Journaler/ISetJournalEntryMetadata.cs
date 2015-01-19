namespace Raft.Infrastructure.Journaler
{
    interface ISetJournalEntryMetadata
    {
        /// <summary>
        /// Merges the journal entry and the metadata which can be extracted using <see cref="IGetJournalEntryMetadata"/>.
        /// </summary>
        /// <returns>The bytes(including the metadata) which should be written to disk.</returns>
        byte[] SetMetadata(EntryMetadata metadata, byte[] journalEntry);
    }
}
