using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Raft.Infrastructure.Journaler.Extensions;

namespace Raft.Infrastructure.Journaler.Readers
{
    // TODO: Extract generic interface and move into contracts project.
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

        /// <summary>
        /// Journal Layout for a single Entry:
        /// |Data Length(int)               |
        /// |MetadataLength(int)(0 if null) |
        /// |Metadata(string)(optional)     |
        /// |Data                           |
        /// |Padding(optional)              |
        /// </summary>
        private IEnumerable<JournalReadResult> ReadEntries()
        {
            foreach (var journalIdxPath in _journalIndexPathMap)
            {
                _currentJournalIndex = journalIdxPath.Key;

                SetJournalFile(journalIdxPath.Value);

                while (_fileStream.Position < _fileStream.Length)
                {
                    using (var binaryReader = new BinaryReader(_fileStream))
                    {
                        var metadata = new Dictionary<string, string>();
                        byte[] entry;

                        try
                        {
                            var entryLength = binaryReader.ReadInt32();
                            if (entryLength == 0) break;

                            var metadataLength = binaryReader.ReadInt32();
                            if (metadataLength != 0)
                            {
                                var metadataBytes = binaryReader.ReadBytes(metadataLength);
                                var metadataString = Encoding.Default.GetString(metadataBytes);
                                metadata.PopulateFrom(metadataString);
                            }

                            entry = binaryReader.ReadBytes(entryLength);
                        }
                        catch (EndOfStreamException) { break; }

                        while (_fileStream.Position < _fileStream.Length)
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
                throw new DirectoryNotFoundException("Could not find journal directory at path: " + configuration.JournalDirectory);

            var files = Directory.GetFiles(configuration.JournalDirectory, configuration.JournalFileName + ".*");
            if (!files.Any())
                throw new FileNotFoundException("No journal files found at path: " + configuration.JournalDirectory);

            Array.ForEach(files, filePath =>
            {
                var idxString = Path.GetExtension(filePath);
                int idx;
                if (!int.TryParse(idxString, out idx))
                    return;

                journalIndexPathMap.Add(idx, filePath);
            });

            if (!journalIndexPathMap.Any())
                throw new FileLoadException("Failed to find any valid journal files to load at path: " + configuration.JournalDirectory);

            var orderedJournalIndexPathMap = journalIndexPathMap
                .OrderBy(x => x.Key)
                .ToDictionary(k => k.Key, v => v.Value);

            if (validateJournalSequence)
            {
                var keys = orderedJournalIndexPathMap.Keys.ToArray();
                if (Enumerable.Range(0, keys.Length).Any(i => keys[i] != keys[0] + i))
                    throw new FileLoadException(
                        "The loaded journal files were not in sequential order. " +
                        "Please ensure the specified journal directory contains all journal files created by the journal object.");
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
}
