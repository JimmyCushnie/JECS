using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SUCC
{
    /// <summary>
    /// Represents a SUCC file in system storage.
    /// </summary>
    public class DataFile : DataFileBase
    {
        /// <summary> Rules for how to format new data saved to this file </summary>
        public FileStyle Style = FileStyle.Default;

        /// <summary> Whether the file will automatically save changes to disk with each Get() or Set(). If false, you must call SaveAllData() manually. </summary>
        /// <remarks> Be careful with this. You do not want to accidentally be writing to a user's disk at 1000MB/s for 3 hours. </remarks>
        public bool AutoSave { get; set; }


        /// <summary>
        /// Creates a new DataFile object corresponding to a SUCC file in system storage.
        /// </summary>
        /// <param name="path"> the path of the file. Can be either absolute or relative to the default path. </param>
        /// <param name="defaultFile"> optionally, if there isn't a file at the path, one can be created from a file in the Resources folder. </param>
        /// <param name="autoSave"> if true, the file will automatically save changes to disk with each Get() or Set(). Otherwise, you must call SaveAllData() manually. </param>
        public DataFile(string path, string defaultFile = null, bool autoSave = false) : this(path, FileStyle.Default, defaultFile, autoSave) { }

        /// <summary>
        /// Creates a new DataFile object corresponding to a SUCC file in system storage, with the option to have a custom FileStyle.
        /// </summary>
        /// <param name="path"> the path of the file. Can be either absolute or relative to the default path. </param>
        /// <param name="style"> the rules for how this file styles newly saved data </param>
        /// <param name="defaultFile"> optionally, if there isn't a file at the path, one can be created from a file in the Resources folder. </param>
        /// <param name="autoSave"> if true, the file will automatically save changes to disk with each Get() or Set(). Otherwise, you must call SaveAllData() manually. </param>
        public DataFile(string path, FileStyle style, string defaultFile = null, bool autoSave = false) : base(path, defaultFile)
        {
            AutoSave = autoSave;
            Style = style;
        }



        /// <summary> Serializes the data in this object to the file on disk. </summary>
        public void SaveAllData()
        {
            string SUCC = GetRawText();

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                string ExistingSUCC = PlayerPrefs.GetString(FilePath);

                if (SUCC != ExistingSUCC)
                    PlayerPrefs.SetString(FilePath, SUCC);
            }
            else
            {
                string ExistingSUCC = File.ReadAllText(FilePath);

                if (SUCC != ExistingSUCC)
                    File.WriteAllText(FilePath, SUCC);
            }
        }


        /// <summary> Get some data from the file, saving a new value if the data does not exist </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="defaultValue"> if the key does not exist in the file, this value is saved there and returned </param>
        public override T Get<T>(string key, T defaultValue = default) => base.Get(key, defaultValue);

        /// <summary> Non-generic version of Get. You probably want to use the other one. </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="DefaultValue"> if the key does not exist in the file, this value is saved there and returned </param>
        public override object Get(Type type, string key, object DefaultValue)
        {
            if (!KeyExists(key))
            {
                Set(type, key, DefaultValue);
                return DefaultValue;
            }

            var node = TopLevelNodes[key];
            return NodeManager.GetNodeData(node, type);
        }

        /// <summary> Save data to the file </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="value"> the value to save </param>
        public void Set<T>(string key, T value) => Set(typeof(T), key, value);

        /// <summary> Non-generic version of Set. You probably want to use the other one. </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="value"> the value to save </param>
        public void Set(Type type, string key, object value)
        {
            if (value == null)
                throw new Exception("you can't serialize null");

            if (value.GetType() != type)
                throw new InvalidCastException($"{value} is not of type {type}!");

            if (!KeyExists(key))
            {
                if (string.IsNullOrEmpty(key))
                    throw new FormatException("SUCC keys must contain at least one character");
                if (key[0] == '-')
                    throw new FormatException("SUCC keys may not begin with the character '-'");
                if (key.Contains(':'))
                    throw new FormatException("SUCC keys may not contain the character ':'");
                if (key.Contains('#'))
                    throw new FormatException("SUCC keys may not contain the character '#'");
                if (key.ContainsNewLine())
                    throw new FormatException("SUCC keys cannot contain a newline");
                if (key[0] == ' ' || key[key.Length - 1] == ' ')
                    throw new FormatException("SUCC keys may not start or end with a space");

                var newnode = new KeyNode(indentation: 0, key, file: this);
                TopLevelNodes.Add(key, newnode);
                TopLevelLines.Add(newnode);
            }

            var node = TopLevelNodes[key];
            NodeManager.SetNodeData(node, value, type, Style);

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

        /// <summary>
        /// Save this file as an object of type T, using that type's fields and properties as top-level keys.
        /// </summary>
        public void SaveAsObject<T>(T savethis) => SaveAsObject(typeof(T), savethis);
        private void SaveAsObject(Type type, object savethis)
        {
            bool _autosave = AutoSave;
            AutoSave = false; // don't write to disk when we don't have to

            try
            {
                var fields = type.GetFields();
                foreach (var f in fields)
                {
                    if (f.IsInitOnly || f.IsLiteral || f.IsPrivate || f.IsStatic) continue;
                    if (Attribute.IsDefined(f, typeof(DontSaveAttribute))) continue;

                    Set(f.FieldType, f.Name, f.GetValue(savethis));
                }

                var properties = type.GetProperties();
                foreach (var p in properties)
                {
                    if (!p.CanRead || !p.CanWrite || p.GetIndexParameters().Length > 0) continue;
                    if (Attribute.IsDefined(p, typeof(DontSaveAttribute))) continue;

                    Set(p.PropertyType, p.Name, p.GetValue(savethis));
                }
            }
            finally
            {
                AutoSave = _autosave;
            }

            if (AutoSave) SaveAllData();
        }

        /// <summary>
        /// Save this file as a dictionary, using the dictionary's keys as top-level keys in the file.
        /// </summary>
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
                    if (keyText.ContainsNewLine()) throw new Exception($"can't save this file as a dictionary; a key contains a new line ({keyText})");
                    if (keyText.Contains('#')) throw new Exception($"can't save this file as a dictionary; a key contains a comment indicator ({keyText})");
                    keyText = keyText.Quote();

                    CurrentKeys.Add(keyText);
                    Set(keyText, dictionary[key]);
                }

                // make sure that old data in the file is deleted when a new dictionary is saved.
                foreach (var key in this.GetTopLevelKeys())
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