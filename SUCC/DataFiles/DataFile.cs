using SUCC.Abstractions;
using System;
using System.IO;

namespace SUCC
{
    /// <summary>
    /// Represents a SUCC file in system storage.
    /// </summary>
    public class DataFile : ReadableWritableDataFile, IDataFileOnDisk
    {
        /// <summary>
        /// Creates a new DataFile object corresponding to a SUCC file in system storage.
        /// </summary>
        /// <param name="path"> The path of the file. Can be either absolute or relative to the default path. </param>
        /// <param name="defaultFileText"> If there isn't already a file at the path, one can be created from the text supplied here. </param>
        public DataFile(string path, string defaultFileText = null) : base(defaultFileText)
        {
            path = Utilities.AbsoluteSuccPath(path);
            this.FilePath = path;

            if (!Utilities.SuccFileExists(path))
            {
                Directory.CreateDirectory(new FileInfo(path).Directory.FullName);

                if (defaultFileText == null)
                    File.Create(path).Close(); // create empty file on disk
                else
                    File.WriteAllText(path, defaultFileText);
            }

            this.ReloadAllData();

            SetupWatcher(); // setup watcher AFTER file has been created
        }

        /// <inheritdoc/>
        protected override string GetSavedText()
        {
            if (File.Exists(FilePath))
                return File.ReadAllText(FilePath);

            return String.Empty;
        }

        /// <inheritdoc/>
        protected override void SetSavedText(string text)
        {
            File.WriteAllText(FilePath, text);

            // FileSystemWatcher.Changed takes several seconds to fire, so we use this.
            IgnoreNextFileReload = true;
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
        /// <inheritdoc/>
        public event Action OnAutoReload;

        /// <inheritdoc/>
        public bool AutoReload
        {
            get => _AutoReload;
            set
            {
                _AutoReload = value;
                Watcher.EnableRaisingEvents = value;

                if (value == true)
                    IgnoreNextFileReload = false; // in case this was set to true while AutoReload was false
            }
        }
        bool _AutoReload = false;

        private FileSystemWatcher Watcher;
        private void SetupWatcher()
        {
            var info = new FileInfo(FilePath);
            Watcher = new FileSystemWatcher(path: info.DirectoryName, filter: info.Name);

            Watcher.NotifyFilter = NotifyFilters.LastWrite;
            Watcher.Changed += this.OnWatcherChanged;
            Watcher.EnableRaisingEvents = this.AutoReload;
        }

        // Watcher.Changed takes several seconds to fire, so we use this.
        private bool IgnoreNextFileReload;

        private void OnWatcherChanged(object idontcare, FileSystemEventArgs goaway)
        {
            if (!_AutoReload)
                return;

            if (IgnoreNextFileReload)
            {
                IgnoreNextFileReload = false;
                return;
            }

            ReloadAllData();
            OnAutoReload?.Invoke();
        }

        #endregion
    }
}