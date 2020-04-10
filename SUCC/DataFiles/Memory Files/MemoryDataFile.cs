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
        /// <remarks> why would you do this? </remarks>
        public MemoryDataFile() : this(string.Empty)
        {
        }

        /// <summary>
        /// Creates a DataFile in memory with some preexisting SUCC content.
        /// </summary>
        public MemoryDataFile(string rawFileText) : this(rawFileText, rawFileText)
        {
        }

        /// <summary>
        /// Advanced constructor, with an identifier for the file and default file text.
        /// </summary>
        public MemoryDataFile(string rawFileText, string identifier, string defaultFileText = null) : base(defaultFileText)
        {
            MemoryTextData = rawFileText;
            Identifier = identifier;
            this.ReloadAllData();
        }


        /// <inheritdoc/>
        public override string Identifier { get; }

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