using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SUCC.Types;

namespace SUCC
{
    /// <summary>
    /// Represents a SUCC file in system storage.
    /// </summary>
    public class DataFile : DataFileBase
    {
        /// <summary> Rules for how to format new data saved to this file </summary>
        public FileStyle Style = FileStyle.Default;

        /// <summary> If true, the DataFile will automatically save changes to disk with each Get or Set. If false, you must call SaveAllData() manually. </summary>
        /// <remarks> Be careful with this. You do not want to accidentally be writing to a user's disk at 1000MB/s for 3 hours. </remarks>
        public bool AutoSave { get; set; }


        /// <summary>
        /// Creates a new DataFile object corresponding to a SUCC file in system storage.
        /// </summary>
        /// <param name="path"> the path of the file. Can be either absolute or relative to the default path. </param>
        /// <param name="defaultFile"> optionally, if there isn't a file at the path, one can be created from a file in the Resources folder. </param>
        /// <param name="autoSave"> if true, the file will automatically save changes to disk with each Get() or Set(). Otherwise, you must call SaveAllData() manually. </param>
        /// <param name="autoReload"> if true, the DataFile will automatically reload when the file changes on disk. </param>
        public DataFile(string path, string defaultFile = null, bool autoSave = true, bool autoReload = false) : this(path, FileStyle.Default, defaultFile, autoSave, autoReload) { }

        /// <summary>
        /// Creates a new DataFile object corresponding to a SUCC file in system storage, with the option to have a custom FileStyle.
        /// </summary>
        /// <param name="path"> the path of the file. Can be either absolute or relative to the default path. </param>
        /// <param name="style"> the rules for how this file styles newly saved data </param>
        /// <param name="defaultFile"> optionally, if there isn't a file at the path, one can be created from a file in the Resources folder. </param>
        /// <param name="autoSave"> if true, the DataFile will automatically save changes to disk with each Get or Set. Otherwise, you must call SaveAllData() manually. </param>
        /// <param name="autoReload"> if true, the DataFile will automatically reload when the file changes on disk. </param>
        public DataFile(string path, FileStyle style, string defaultFile = null, bool autoSave = true, bool autoReload = false) : base(path, defaultFile, autoReload)
        {
            AutoSave = autoSave;
            Style = style;
        }



        /// <summary> Serializes the data in this object to the file on disk. </summary>
        public void SaveAllData()
        {
            string SUCC = GetRawText();
            string ExistingSUCC = string.Empty;

#if UNITY_WEBGL
            ExistingSUCC = PlayerPrefs.GetString(FilePath);

            if (SUCC != ExistingSUCC)
                PlayerPrefs.SetString(FilePath, SUCC);

#else
            if (File.Exists(FilePath)) // in case the file is deleted between when the file is initialized and when it's saved
                ExistingSUCC = File.ReadAllText(FilePath);

            if (SUCC != ExistingSUCC)
            {
                File.WriteAllText(FilePath, SUCC);

                // FileSystemWatcher.Chagned takes several seconds to fire, so we use this.
                IgnoreNextFileReload = true;
            }
#endif
        }


        /// <summary> Get some data from the file, saving a new value if the data does not exist </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="defaultValue"> if the key does not exist in the file, this value is saved there and returned </param>
        public override T Get<T>(string key, T defaultValue = default) => base.Get(key, defaultValue);

        /// <summary> Non-generic version of Get. You probably want to use Get. </summary>
        /// <param name="type"/> the type to get the data as </param>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="DefaultValue"> if the key does not exist in the file, this value is saved there and returned </param>
        public override object GetNonGeneric(Type type, string key, object DefaultValue)
        {
            if (!KeyExists(key))
            {
                SetNonGeneric(type, key, DefaultValue);
                return DefaultValue;
            }

            var node = TopLevelNodes[key];
            return NodeManager.GetNodeData(node, type);
        }

        /// <summary> Save data to the file </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="value"> the value to save </param>
        public void Set<T>(string key, T value) => SetNonGeneric(typeof(T), key, value);

        /// <summary> Non-generic version of Set. You probably want to use Set. </summary>
        /// <param name="type"> the type to save the data as </param>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="value"> the value to save </param>
        public void SetNonGeneric(Type type, string key, object value)
        {
            if (value == null)
                throw new Exception("you can't serialize null");

            if (value.GetType() != type)
                throw new InvalidCastException($"{value} is not of type {type}!");

            if (!KeyExists(key))
            {
                var newnode = new KeyNode(indentation: 0, key, file: this);
                TopLevelNodes.Add(key, newnode);
                TopLevelLines.Add(newnode);
            }

            var node = TopLevelNodes[key];
            NodeManager.SetNodeData(node, value, type, Style);

            if (AutoSave)
                SaveAllData();
        }

        /// <summary> Like Get but works for nested paths instead of just the top level of the file </summary>
        public override T GetAtPath<T>(T defaultValue = default, params string[] path) => base.GetAtPath(defaultValue, path);
        /// <summary> Like Get but works for nested paths instead of just the top level of the file </summary>
        public override object GetAtPathNonGeneric(Type type, object defaultValue, params string[] path)
        {
            if (!KeyExistsAtPath(path)) // throws exception for us when path.length < 1
            {
                SetAtPathNonGeneric(type, defaultValue, path);
                return defaultValue;
            }
            
            var topNode = TopLevelNodes[path[0]];
            for (int i = 1; i < path.Length; i++)
            {
                topNode = topNode.GetChildAddressedByName(path[i]);
            }

            return NodeManager.GetNodeData(topNode, type);
        }

        /// <summary> Like Set but works for nested paths instead of just the top level of the file </summary>
        public void SetAtPath<T>(T value, params string[] path) => SetAtPathNonGeneric(typeof(T), value, path);
        /// <summary> Like Set but works for nested paths instead of just the top level of the file </summary>
        public void SetAtPathNonGeneric(Type type, object value, params string[] path)
        {
            if (value == null)
                throw new Exception("you can't serialize null");

            if (value.GetType() != type)
                throw new InvalidCastException($"{value} is not of type {type}!");

            if (path.Length < 1)
                throw new ArgumentException($"{nameof(path)} must have a length greater than 0");


            if (!KeyExists(path[0]))
            {
                var newnode = new KeyNode(indentation: 0, key: path[0], file: this);
                TopLevelNodes.Add(path[0], newnode);
                TopLevelLines.Add(newnode);
            }

            var topNode = TopLevelNodes[path[0]];
            
            for (int i = 1; i < path.Length; i++)
            {
                topNode = topNode.GetChildAddressedByName(path[i]);
            }

            NodeManager.SetNodeData(topNode, value, type, Style);
            
            if (AutoSave)
                SaveAllData();
        }


        /// <summary> Remove a top-level key and all its data from the file </summary>
        public void DeleteKey(string key)
        {
            if (!KeyExists(key))
                return;

            Node node = TopLevelNodes[key];
            TopLevelNodes.Remove(key);
            TopLevelLines.Remove(node);
        }


        /// <summary> Save this file as an object of type T, using that type's fields and properties as top-level keys. </summary>
        public void SaveAsObject<T>(T savethis) => SaveAsObjectNonGeneric(typeof(T), savethis);

        /// <summary> Non-generic version of SaveAsObject. You probably want to use SaveAsObject. </summary>
        /// <param name="type"> what type to save this object as </param>
        /// <param name="savethis"> the object to save </param>
        public void SaveAsObjectNonGeneric(Type type, object savethis)
        {
            bool _autosave = AutoSave;
            AutoSave = false; // don't write to disk when we don't have to

            try
            {
                foreach (var f in ComplexTypes.GetValidFields(type))
                    SetNonGeneric(f.FieldType, f.Name, f.GetValue(savethis));

                foreach (var p in ComplexTypes.GetValidProperties(type))
                    SetNonGeneric(p.PropertyType, p.Name, p.GetValue(savethis));
            }
            finally
            {
                AutoSave = _autosave;
            }

            if (AutoSave) SaveAllData();
        }

        /// <summary> Save this file as a dictionary, using the dictionary's keys as top-level keys in the file. </summary>
        /// <remarks> TKey must be a Base Type </remarks>
        public void SaveAsDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            if (!BaseTypes.IsBaseType(typeof(TKey)))
                throw new Exception("When using GetAsDictionary, TKey must be a base type");

            bool _autosave = AutoSave;
            AutoSave = false; // don't write to disk when we don't have to

            try
            {
                var CurrentKeys = new List<string>(capacity: dictionary.Count);
                foreach (var key in dictionary.Keys)
                {
                    var keyText = BaseTypes.SerializeBaseType(key, Style);
                    if (!Utilities.IsValidKey(keyText, out string whyNot))
                        throw new Exception($"can't save file as this dictionary. A key ({keyText}) is not valid: {whyNot}");

                    CurrentKeys.Add(keyText);
                    Set(keyText, dictionary[key]);
                }

                // make sure that old data in the file is deleted when a new dictionary is saved.
                foreach (var key in this.TopLevelKeys)
                {
                    if (!CurrentKeys.Contains(key))
                        this.TopLevelNodes.Remove(key);
                }
            }
            finally
            {
                AutoSave = _autosave;
            }

            if (AutoSave) SaveAllData();
        }
    }
}