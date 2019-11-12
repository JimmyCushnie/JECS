using SUCC.Abstractions;
using System;
using System.IO;

namespace SUCC
{
    /// <summary>
    /// A read-only version of DataFile. Data can be read from disk, but not saved to disk.
    /// </summary>
    public class ReadOnlyDataFile : ReadableDataFile, IDataFileOnDisk
    {
        /// <summary>
        /// Creates a new ReadOnlyDataFile object corresponding to a SUCC file in system storage.
        /// </summary>
        /// <param name="path"> the path of the file. Can be either absolute or relative to the default path. </param>
        /// <param name="defaultFileText"> optionally, if there isn't a file at the path, one can be created from the text supplied here. </param>
        /// <param name="autoReload"> if true, the DataFile will automatically reload when the file changes on disk. </param>
        public ReadOnlyDataFile(string path, string defaultFileText = null, bool autoReload = false)
        {
            path = Utilities.AbsolutePath(path);
            path = Path.ChangeExtension(path, Utilities.FileExtension);
            this.FilePath = path;

            if (!Utilities.SuccFileExists(path))
            {
                if (defaultFileText == null)
                {
                    Directory.CreateDirectory(new FileInfo(path).Directory.FullName);
                    File.Create(path).Close(); // create empty file on disk
                }
                else
                {
                    File.WriteAllText(path, defaultFileText);
                }
            }

            this.ReloadAllData();

            SetupWatcher(); // setup watcher AFTER file has been created
            this.AutoReload = autoReload;
        }

        /// <inheritdoc/>
        protected override string GetSavedText()
        {
            if (File.Exists(FilePath))
                return File.ReadAllText(FilePath);

            return String.Empty;
        }



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
        bool _AutoReload = true;

        private FileSystemWatcher Watcher;
        private void SetupWatcher()
        {
            var info = new FileInfo(FilePath);
            Watcher = new FileSystemWatcher(path: info.DirectoryName, filter: info.Name);

            Watcher.NotifyFilter = NotifyFilters.LastWrite;
            Watcher.Changed += this.OnWatcherChanged;
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