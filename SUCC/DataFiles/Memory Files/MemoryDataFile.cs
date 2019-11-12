using SUCC.Abstractions;
using System.IO;

namespace SUCC.MemoryFiles
{
    /// <summary>
    /// A read-only version of DataFile. Data can be read from disk, but not saved to disk.
    /// </summary>
    public class MemoryDataFile : ReadableWritableDataFile
    {
        /// <summary>
        /// Creates an empty DataFile in memory.
        /// </summary>
        public MemoryDataFile() : this(string.Empty) { }

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
            this.ReloadAllData();
        }


        private string MemoryTextData = "";

        /// <inheritdoc/>
        protected override string GetSavedText()
            => MemoryTextData;

        /// <inheritdoc/>
        protected override void SetSavedText(string text)
            => MemoryTextData = text;


        /// <summary>
        /// Saves the contents of this MemoryDataFile to disk and returns a disk DataFile corresponding to the new file.
        /// </summary>
        /// <param name="relativeOrAbsolutePath"> The path of the new file. </param>
        /// <param name="overwrite"> If this is false, don't save the data if the file already exists on disk. </param>
        /// <returns> Null if overwrite was set to false and a file already existed </returns>
        public DataFile ConvertToFileOnDisk(string relativeOrAbsolutePath, bool overwrite = true)
        {
            if (!overwrite && Utilities.SuccFileExists(relativeOrAbsolutePath))
                return null;

            string path = Utilities.AbsolutePath(relativeOrAbsolutePath);
            File.WriteAllText(path, GetRawText());
            return new DataFile(path);
        }
    }
}