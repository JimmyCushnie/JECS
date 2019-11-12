using System;

namespace SUCC
{
    /// <summary>
    /// A read-only version of DataFile. Data can be read from disk, but not saved to disk.
    /// </summary>
    public class MemoryReadOnlyDataFile : ReadableDataFile
    {
        /// <summary>
        /// Creates an empty ReadOnlyDataFile in memory.
        /// </summary>
        public MemoryReadOnlyDataFile() : this(String.Empty) { }

        /// <summary>
        /// Creates a ReadOnlyDataFile in memory with some preexisting SUCC content.
        /// </summary>
        public MemoryReadOnlyDataFile(string rawFileText)
        {
            MemoryTextData = rawFileText;
        }


        private string MemoryTextData = "";

        protected override string GetSavedText()
            => MemoryTextData;
    }
}