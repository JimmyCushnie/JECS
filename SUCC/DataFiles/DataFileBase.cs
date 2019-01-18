using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SUCC
{
    public abstract class DataFileBase
    {
        /// <summary> The absolute path of the file this object corresponds to. </summary>
        public readonly string FilePath;

        public DataFileBase(string path, string defaultFile, bool autoReload)
        {
            path = Utilities.AbsolutePath(path);
            path = Path.ChangeExtension(path, Utilities.FileExtension);
            this.FilePath = path;

            SetupWatcher();
            this.AutoReload = autoReload;

            if (Utilities.SuccFileExists(path))
            {
                this.ReloadAllData();
            }
            else
            {
                if (defaultFile != null)
                {
                    var textFile = Resources.Load<TextAsset>(defaultFile);
                    if (textFile == null)
                        throw new Exception("The default file you specified doesn't exist in Resources :(");

                    writeFile(textFile);
                    Resources.UnloadAsset(textFile);
                }
                else if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    File.Create(path).Close(); // create empty file on disk
                }
            }

            void writeFile(TextAsset file)
            {
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                    PlayerPrefs.SetString(path, file.text);
                else
                    File.WriteAllBytes(path, file.bytes);
            }
        }

        internal List<Line> TopLevelLines { get; private set; } = new List<Line>();
        internal Dictionary<string, KeyNode> TopLevelNodes { get; private set; } = new Dictionary<string, KeyNode>();

        /// <summary> Reloads the data stored on disk into this object. </summary>
        public void ReloadAllData()
        {
            try
            {
                string succ;
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                    succ = PlayerPrefs.GetString(FilePath);
                else
                    succ = File.ReadAllText(FilePath);

                var data = DataConverter.DataStructureFromSUCC(succ, this);
                TopLevelLines = data.Item1;
                TopLevelNodes = data.Item2;
            }
            catch(Exception e)
            {
                throw new Exception($"error parsing data from file at path {FilePath}: {e.Message}");
            }
        }

        /// <summary> gets the data as it appears in file </summary>
        public string GetRawText() 
            => DataConverter.SUCCFromDataStructure(TopLevelLines);

        /// <summary> gets the data as it appears in file, as an array of strings (one for each line) </summary>
        public string[] GetRawLines()
            => GetRawText().SplitIntoLines();

        /// <summary> returns all top level keys in the file, in order. </summary>
        public string[] GetTopLevelKeys()
        {
            var keys = new string[TopLevelNodes.Count];
            int count = 0;
            foreach (var line in TopLevelLines)
            {
                if (line is KeyNode)
                {
                    var node = (KeyNode)line;
                    keys[count] = node.Key;
                    count++;
                }
            }

            return keys;
        }

        /// <summary> whether a top-level key exists in the file </summary>
        public bool KeyExists(string key)
        {
            return TopLevelNodes.ContainsKey(key);
        }

        // Get<T> is virual so that derived classes can give it different xml documentation
        public virtual T Get<T>(string key, T defaultValue) => (T)Get(typeof(T), key, defaultValue);
        public abstract object Get(Type type, string key, object defaultValue);


        /// <summary>
        /// Interpret this file as an object of type T, using that type's fields and properties as top-level keys.
        /// </summary>
        public T GetAsObject<T>() => (T)GetAsObject(typeof(T));
        private object GetAsObject(Type type)
        {
            object returnThis = Activator.CreateInstance(type);

            var fields = type.GetFields();
            foreach (var f in fields)
            {
                if (f.IsInitOnly || f.IsLiteral || f.IsPrivate || f.IsStatic) continue;
                if (Attribute.IsDefined(f, typeof(DontSaveAttribute))) continue;

                var value = Get(f.FieldType, f.Name, f.GetValue(returnThis));
                f.SetValue(returnThis, value);
            }

            var properties = type.GetProperties();
            foreach (var p in properties)
            {
                if (!p.CanRead || !p.CanWrite || p.GetIndexParameters().Length > 0) continue;
                if (Attribute.IsDefined(p, typeof(DontSaveAttribute))) continue;

                var value = Get(p.PropertyType, p.Name, p.GetValue(returnThis));
                p.SetValue(returnThis, value);
            }

            return returnThis;
        }

        /// <summary>
        /// Interpret this file as a dictionary. Top-level keys in the file are interpreted as keys in the dictionary.
        /// </summary>
        /// <remarks> TKey must be a Base Type </remarks>
        public Dictionary<TKey, TValue> GetAsDictionary<TKey, TValue>()
        {
            if (!BaseTypes.IsBaseType(typeof(TKey)))
                throw new Exception("When using GetAsDictionary, TKey must be a base type");

            var keys = GetTopLevelKeys();
            var dictionary = new Dictionary<TKey, TValue>(capacity: keys.Length);

            foreach (var keyText in keys)
            {
                TKey key = BaseTypes.ParseBaseType<TKey>(keyText);
                TValue value = NodeManager.GetNodeData<TValue>(TopLevelNodes[keyText]);
                dictionary.Add(key, value);
            }

            return dictionary;
        }


        bool _AutoReload = true;
        /// <summary> If true, the DataFile will automatically reload when the file changes on disk. If false, you can still call ReloadAllData().
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

        private FileSystemWatcher Watcher;
        private void SetupWatcher()
        {
            var info = new FileInfo(FilePath);
            Watcher = new FileSystemWatcher(path: info.DirectoryName, filter: info.Name);

            Watcher.NotifyFilter = NotifyFilters.LastWrite;
            Watcher.Changed += this.OnFileAutoReloadWrapper;
        }

        // Watcher.Changed takes several seconds to fire, so we use this.
        protected bool IgnoreNextFileReload;

        private void OnFileAutoReloadWrapper(object idontcare, FileSystemEventArgs goaway)
        {
            if (!_AutoReload) return;
            if (IgnoreNextFileReload)
            {
                IgnoreNextFileReload = false;
                return;
            }

            ReloadAllData();
            OnAutoReload?.Invoke();
        }

        /// <summary>
        /// Invoked every time the file is auto-reloaded. This only happens when AutoReload is true.
        /// </summary>
        public event Action OnAutoReload;
    }
}