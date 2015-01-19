using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Raft.Infrastructure.Journaler.Reader
{
    public class JournalReader : IDisposable
    {
        private readonly IDictionary<int, string> _journalIndexPathMap;

        private FileStream _fileStream;
        private int _currentJournalIndex;
        private long _currentJournalEntryIndex;


        public JournalReader(JournalConfiguration configuration, bool validateJournalSequence)
        {
            _journalIndexPathMap = GetJournalFileIndexMap(configuration, validateJournalSequence);
        }

        private IEnumerable<JournalReadResult> ReadEntries()
        {
            foreach (var journalIdxPath in _journalIndexPathMap)
            {
                _currentJournalIndex = journalIdxPath.Key;

                SetJournalFile(journalIdxPath.Value);

                while (_fileStream.Position != _fileStream.Length)
                {
                    using (var binaryReader = new BinaryReader(_fileStream))
                    {
                        var entryLength = binaryReader.ReadInt32();
                        var entry = binaryReader.ReadBytes(entryLength);

                        while (_fileStream.Position != _fileStream.Length)
                        {
                            var nextByte = _fileStream.ReadByte();
                            if (nextByte == 0) continue;

                            _fileStream.Position = _fileStream.Position - 1;
                            break;
                        }

                        yield return new JournalReadResult(_currentJournalIndex, _currentJournalEntryIndex, entry);

                        _currentJournalEntryIndex++;
                    }
                }
            }
        }

        private static Dictionary<int, string> GetJournalFileIndexMap(JournalConfiguration configuration, bool validateJournalSequence)
        {
            var journalIndexPathMap = new Dictionary<int, string>();

            if (!Directory.Exists(configuration.JournalDirectory))
                throw new DirectoryNotFoundException(); // Directory doesn't exist...

            var files = Directory.GetFiles(configuration.JournalDirectory, configuration.JournalFileName + ".*");
            if (!files.Any())
                throw new FileNotFoundException(); // No files in directory...

            Array.ForEach(files, filePath =>
            {
                var idxString = Path.GetExtension(filePath);
                int idx;
                if (!int.TryParse(idxString, out idx))
                    return;

                journalIndexPathMap.Add(idx, filePath);
            });

            if (!journalIndexPathMap.Any())
                throw new Exception(); // Could not parse journal file names...

            var orderedJournalIndexPathMap = journalIndexPathMap
                .OrderBy(x => x.Key)
                .ToDictionary(k => k.Key, v => v.Value);

            if (validateJournalSequence)
            {
                var keys = orderedJournalIndexPathMap.Keys.ToArray();
                if (Enumerable.Range(0, keys.Length).Any(i => keys[i] != keys[0] + i))
                    throw new Exception(); // Could not validate journal file sequence...
            }

            return orderedJournalIndexPathMap;
        }

        private void SetJournalFile(string filePath)
        {
            CloseCurrentStream();

            _fileStream = new FileStream(filePath, FileMode.Open,
                FileAccess.Read, FileShare.None, 2 << 11,
                FileOptions.SequentialScan);

            _fileStream.Seek(0, SeekOrigin.Current);
        }

        private void CloseCurrentStream()
        {
            if (_fileStream == null) return;

            _fileStream.Dispose();
            _fileStream = null;
        }

        public void Dispose()
        {
            CloseCurrentStream();
        }
    }

    public class JournalReadResult
    {
        public int JournalIndex { get; private set; }
        public long Index { get; private set; }
        public byte[] Entry { get; private set; }

        public JournalReadResult(int journalIndex, long index, byte[] entry)
        {
            JournalIndex = journalIndex;
            Index = index;
            Entry = entry;
        }
    }
}
