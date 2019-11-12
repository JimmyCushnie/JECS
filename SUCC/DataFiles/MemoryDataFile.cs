using System;

namespace SUCC
{
    /// <summary>
    /// A read-only version of DataFile. Data can be read from disk, but not saved to disk.
    /// </summary>
    public class MemoryDataFile : WritableDataFile
    {
        /// <summary>
        /// Creates an empty DataFile in memory.
        /// </summary>
        public MemoryDataFile() : this(String.Empty) { }

        /// <summary>
        /// Creates a DataFile in memory with some preexisting SUCC content.
        /// </summary>
        public MemoryDataFile(string rawFileText, bool autoSave = true) : this(rawFileText, FileStyle.Default, autoSave) { }

        /// <summary>
        /// Creates a DataFile in memory with some preexisting SUCC content and a custom FileStyle.
        /// </summary>
        public MemoryDataFile(string rawFileText, FileStyle style, bool autoSave = true) : base(autoSave, style)
        {
            MemoryTextData = rawFileText;
        }


        private string MemoryTextData = "";

        protected override string GetSavedText()
            => MemoryTextData;

        protected override void SetSavedText(string text)
            => MemoryTextData = text;
    }
}