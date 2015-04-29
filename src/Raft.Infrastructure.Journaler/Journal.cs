using System;
using System.Collections.Generic;
using System.Linq;
using Raft.Contracts.Persistance;
using Raft.Extensions.Journaler.Transformers;
using Raft.Extensions.Journaler.Writers;

namespace Raft.Extensions.Journaler
{
    internal class Journal : IWriteDataBlocks, IDisposable
    {
        private readonly JournalConfiguration _journalConfiguration;
        private readonly IJournalFileWriter _journalFileWriter;
        private readonly JournalOffsetManager _journalOffsetManager;
        private readonly IList<ITransformJournalEntry> _entryTransformers;
        
        public Journal(JournalConfiguration journalConfiguration, IJournalFileWriter journalFileWriter, JournalOffsetManager journalOffsetManager, IList<ITransformJournalEntry> entryTransformers)
        {
            _journalConfiguration = journalConfiguration;
            _journalFileWriter = journalFileWriter;
            _journalOffsetManager = journalOffsetManager;
            _entryTransformers = entryTransformers;

            _journalFileWriter.SetJournal(_journalOffsetManager.CurrentJournalIndex, _journalOffsetManager.NextJournalEntryOffset);
        }

        public void WriteBlock(DataBlock block)
        {
            WriteBlock(block, true);
        }

        public void WriteBlocks(DataBlock[] blocks)
        {
            for (var i = 0; i < blocks.Length; i++)
            {
                WriteBlock(blocks[i], i == blocks.Length - 1);
            }
        }

        private void WriteBlock(DataBlock block, bool flush)
        {
            var bytes = block.Data;

            _entryTransformers.ToList()
                .ForEach(x => bytes = x.Transform(bytes, block.Metadata));

            if ((_journalOffsetManager.NextJournalEntryOffset + bytes.Length) > _journalConfiguration.LengthInBytes)
            {
                _journalOffsetManager.IncrementJournalIndex();
                _journalFileWriter.SetJournal(_journalOffsetManager.CurrentJournalIndex, _journalOffsetManager.NextJournalEntryOffset);
            }

            _journalOffsetManager.UpdateJournalOffset(bytes.Length);
            _journalFileWriter.WriteJournalEntry(bytes);

            if (flush)
                _journalFileWriter.Flush();
        }

        public void Dispose()
        {
            _journalFileWriter.Dispose();
        }
    }
}
