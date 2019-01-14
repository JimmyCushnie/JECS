using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SUCC
{
    public class DataFile : DataFileBase
    {
        /// <summary> Whether the file will automatically save changes to disk with each Get() or Set(). If false, you must call SaveAllData() manually. </summary>
        public bool AutoSave { get; set; }

        public DataFile(string path, string defaultFile = null, bool autoSave = false) : base(path, defaultFile)
        {
            AutoSave = autoSave;
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
                if (key.Contains(":"))
                    throw new FormatException("SUCC keys may not contain the character ':'");
                if (key.Contains("#"))
                    throw new FormatException("SUCC keys may not contain the character '#'");
                if (key.Contains("\n"))
                    throw new FormatException("SUCC keys cannot contain a newline");
                if (key[0] == ' ' || key[key.Length - 1] == ' ')
                    throw new FormatException("SUCC keys may not start of end with a space");

                var newnode = new KeyNode() { RawText = key + ':' };
                TopLevelNodes.Add(key, newnode);
                TopLevelLines.Add(newnode);
            }

            var node = TopLevelNodes[key];
            NodeManager.SetNodeData(node, value, type);

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


        public void SaveAsObject<T>(T savethis)
        {
            Type type = typeof(T);
            SaveAsObject(type, savethis);
        }

        public void SaveAsObject(Type type, object savethis)
        {
            if (savethis.GetType() != type)
                throw new InvalidOperationException("The type passed to SaveAsObject must match the type of the savethis object");

            var fields = type.GetFields();
            foreach (var f in fields)
            {
                if (f.IsInitOnly || f.IsLiteral || f.IsPrivate || f.IsStatic) { continue; }
                if (Attribute.IsDefined(f, typeof(DontSaveAttribute))) { continue; }

                Set(f.FieldType, f.Name, f.GetValue(savethis));
            }

            var properties = type.GetProperties();
            foreach (var p in properties)
            {
                if (!p.CanRead || !p.CanWrite || p.GetIndexParameters().Length > 0) { continue; }
                if (Attribute.IsDefined(p, typeof(DontSaveAttribute))) { continue; }

                Set(p.PropertyType, p.Name, p.GetValue(savethis));
            }

            SaveAllData();
        }

        public void SaveAsDictionary<TKey, TValue>()
        {
            throw new NotImplementedException();
        }
    }
}