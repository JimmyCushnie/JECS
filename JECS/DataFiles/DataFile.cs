using JECS.Abstractions;
using JECS.UnityStuff;
using System;
using System.IO;

namespace JECS
{
    /// <summary>
    /// Represents a JECS file in system storage.
    /// </summary>
    public class DataFile : ReadableWritableDataFile, IDataFileOnDisk
    {
        /// <summary>
        /// Creates a new <see cref="DataFile"/> object, using a text file in Unity's Resources folder for the default file text.
        /// </summary>
        /// <param name="path"> The path of the file. Can be either absolute or relative to the default path. </param>
        /// <param name="defaultFile"> The path of the default file, relative to any Resources parent. </param>
        /// <returns></returns>
        public static DataFile WithDefaultFile(string path, string defaultFile)
        {
            string defaultFileText = ResourcesUtilities.ReadTextFromFile(defaultFile);
            return new DataFile(path, defaultFileText);
        }


        /// <summary>
        /// Creates a new DataFile object corresponding to a JECS file in system storage.
        /// </summary>
        /// <param name="path"> The path of the file. Can be either absolute or relative to the default path. </param>
        /// <param name="defaultFileText"> If there isn't already a file at the path, one can be created from the text supplied here. </param>
        public DataFile(string path, string defaultFileText = null) : base(defaultFileText)
        {
            path = Utilities.AbsoluteJecsPath(path);
            this.FilePath = path;

            if (!Utilities.JecsFileExists(path))
            {
                Directory.CreateDirectory(new FileInfo(path).Directory.FullName);

                if (defaultFileText == null)
                    File.Create(path).Close(); // create empty file on disk
                else
                    File.WriteAllText(path, defaultFileText);
            }

            this.ReloadAllData();
        }

        /// <inheritdoc/>
        protected override string GetSavedText()
        {
            lock (FileSystemReadWriteLock)
            {
                if (File.Exists(FilePath))
                {
                    LastKnownWriteTimeUTC = this.GetCurrentLastWriteTimeUTC();
                    return File.ReadAllText(FilePath);
                }

                return String.Empty;
            }
        }

        /// <inheritdoc/>
        protected override void SetSavedText(string text)
        {
            lock (FileSystemReadWriteLock)
            {
                File.WriteAllText(FilePath, text);
                LastKnownWriteTimeUTC = this.GetCurrentLastWriteTimeUTC();
            }
        }

        /// <inheritdoc/>
        public override string Identifier => FilePath;



        #region IDataFileOnDisk implementation
        // this code is copied between DataFile and ReadOnlyDataFile.
        // todo: once we upgrade to c# 8, this can probably be abstracted to a default interface implementation.

        /// <inheritdoc/>
        public string FilePath { get; protected set; }
        /// <inheritdoc/>
        public string FileName => Path.GetFileNameWithoutExtension(FilePath);
        /// <inheritdoc/>
        public long SizeOnDisk => new FileInfo(FilePath).Length;


        // For the below members, don't make them directly public; it'll just clutter the public API for this class.

        object IDataFileOnDisk.FileSystemReadWriteLock => FileSystemReadWriteLock;
        private readonly object FileSystemReadWriteLock = new object();

        DateTime IDataFileOnDisk.LastKnownWriteTimeUTC => LastKnownWriteTimeUTC;
        private DateTime LastKnownWriteTimeUTC;

        #endregion
    }
}