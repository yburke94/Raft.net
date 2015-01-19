using System;
using System.Collections.Generic;
using System.Linq;

namespace Raft.Infrastructure.Journaler
{
    internal class Journaler : IJournaler, IDisposable
    {
        private readonly JournalConfiguration _journalConfiguration;
        private readonly IJournalFileWriter _journalFileWriter;
        private readonly JournalOffsetManager _journalOffsetManager;
        private readonly IList<ITransformJournalEntry> _entryTransformers;
        
        public Journaler(JournalConfiguration journalConfiguration, IJournalFileWriter journalFileWriter, JournalOffsetManager journalOffsetManager, IList<ITransformJournalEntry> entryTransformers)
        {
            _journalConfiguration = journalConfiguration;
            _journalFileWriter = journalFileWriter;
            _journalOffsetManager = journalOffsetManager;
            _entryTransformers = entryTransformers;

            _journalFileWriter.SetJournal(_journalOffsetManager.CurrentJournalIndex, _journalOffsetManager.NextJournalEntryOffset);
        }

        public void WriteBlock(byte[] block)
        {
            WriteBlock(block, true);
        }

        public void WriteBlocks(byte[][] blocks)
        {
            for (var i = 0; i < blocks.Length; i++)
            {
                WriteBlock(blocks[i], i == blocks.Length - 1);
            }
        }

        private void WriteBlock(byte[] block, bool flush)
        {
            _entryTransformers.ToList()
                .ForEach(x => block = x.Transform(block));

            if ((_journalOffsetManager.NextJournalEntryOffset + block.Length) > _journalConfiguration.LengthInBytes)
            {
                _journalOffsetManager.IncrementJournalIndex();
                _journalFileWriter.SetJournal(_journalOffsetManager.CurrentJournalIndex);
            }

            _journalOffsetManager.UpdateJournalOffset(block.Length);
            _journalFileWriter.WriteJournalEntry(block);

            if (flush)
                _journalFileWriter.Flush();
        }

        public void Dispose()
        {
            _journalFileWriter.Dispose();

        }
    }
}
