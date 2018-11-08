using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PENIS
{
    public class DataFile
    {
        /// <summary> The absolute path of the file this object corresponds to. </summary>
        public readonly string FilePath;

        /// <summary> creates a new TextFile object, which corresponds to a text file in storage and can be used for easy reference. </summary>
        /// <param name="path"> the path of the file. Can be either absolute or relative to the default path. </param>
        public DataFile(string path, string DefaultFile = null)
        {
            path = Utilities.AbsolutePath(path);
            path = Path.ChangeExtension(path, Utilities.FileExtension);
            this.FilePath = path;

#if UNITY_WEBGL
            if (PlayerPrefs.GetString(path, "") == "" && DefaultFile != null)
            {
                var defaultFile = Resources.Load<TextAsset>(DefaultFile);
                if (defaultFile == null)
                    throw new Exception("The default file you specified doesn't exist in Resources :(");

                PlayerPrefs.SetString(path, defaultFile.text);
                Resources.UnloadAsset(defaultFile);
            }
#else
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (!File.Exists(path))
            {
                if (DefaultFile != null)
                {
                    var defaultFile = Resources.Load<TextAsset>(DefaultFile);
                    if (defaultFile == null)
                        throw new Exception("The default file you specified doesn't exist in Resources :(");

                    File.WriteAllBytes(path, defaultFile.bytes);
                    Resources.UnloadAsset(defaultFile);
                }
                else
                {
                    File.Create(path).Close();
                }
            }
#endif

            this.ReloadAllData();
        }

        public List<Line> TopLevelLines { get; private set; }
        public Dictionary<string, KeyNode> TopLevelNodes { get; private set; }


        /// <summary> Reloads the data stored on disk into this object. </summary>
        public void ReloadAllData()
        {
#if UNITY_WEBGL
            string file = PlayerPrefs.GetString(FilePath);
            var data = DataConverter.DataStructureFromPENIS(file);
            TopLevelLines = data.Item1;
            TopLevelNodes = data.Item2;
#else
            string[] lines = File.ReadAllLines(FilePath);
            var data = DataConverter.DataStructureFromPENIS(lines);
            TopLevelLines = data.Item1;
            TopLevelNodes = data.Item2;
#endif
        }

        /// <summary> Serializes the data in this object to the file on disk. </summary>
        public void SaveAllData()
        {
            string PENIS = DataConverter.PENISFromDataStructure(TopLevelLines);

#if UNITY_WEBGL
            string ExistingPENIS = PlayerPrefs.GetString(FilePath);

            if (PENIS != ExistingPENIS)
                PlayerPrefs.SetString(FilePath, PENIS);
#else
            string ExistingPENIS = File.ReadAllText(FilePath);

            if(PENIS != ExistingPENIS)
                File.WriteAllText(FilePath, PENIS);
#endif
        }


        /// <summary> get some data from the file, saving a new value if the data does not exist </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="DefaultValue"> if the key does not exist in the file, this value is saved there and returned </param>
        public T Get<T>(string key, T DefaultValue = default(T))
        {
            return (T)Get(typeof(T), key, DefaultValue);
        }

        public object Get(Type type, string key, object DefaultValue)
        {
            if (!KeyExists(key))
            {
                Set(type, key, DefaultValue);
                return DefaultValue;
            }

            var node = TopLevelNodes[key];
            return NodeManager.GetNodeData(node, type);
        }

        /// <summary> save data to the file </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="value"> the value to save </param>
        public void Set<T>(string key, T value)
        {
            Set(typeof(T), key, value);
        }

        public void Set(Type type, string key, object value)
        {
            if (value == null)
                throw new Exception("you can't serialize null");

            if (value.GetType() != type)
                throw new InvalidCastException("value is not of type " + type.Name + "!");

            if (!KeyExists(key))
            {
                if (string.IsNullOrEmpty(key))
                    throw new FormatException("PENIS keys must contain at least one character");
                if (key[0] == '-')
                    throw new FormatException("PENIS keys may not begin with the character '-'");
                if (key.Contains(":"))
                    throw new FormatException("PENIS keys may not contain the character ':'");
                if (key.Contains("#"))
                    throw new FormatException("PENIS keys may not contain the character '#'");
                if (key.Contains("\n"))
                    throw new FormatException("PENIS keys cannot contain a newline");
                if (key[0] == ' ' || key[key.Length - 1] == ' ')
                    throw new FormatException("PENIS keys may not start of end with a space");

                var newnode = new KeyNode() { RawText = key + ':' };
                TopLevelNodes.Add(key, newnode);
                TopLevelLines.Add(newnode);
            }

            var node = TopLevelNodes[key];
            NodeManager.SetNodeData(node, value, type);
        }

        /// <summary> returns all top level keys in the file, in order. </summary>
        public string[] GetTopLevelKeys()
        {
            var keys = new string[TopLevelNodes.Count];
            int count = 0;
            foreach(var line in TopLevelLines)
            {
                if(line is KeyNode)
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

        public T GetAsObject<T>()
        {
            return (T)GetAsObject(typeof(T));
        }

        public object GetAsObject(Type type)
        {
            object returnThis = Activator.CreateInstance(type);

            var fields = type.GetFields();
            foreach (var f in fields)
            {
                if (f.IsInitOnly || f.IsLiteral || f.IsPrivate || f.IsStatic) { continue; }
                if (Attribute.IsDefined(f, typeof(DontSaveAttribute))) { continue; }

                var value = Get(f.FieldType, f.Name, f.GetValue(returnThis));
                f.SetValue(returnThis, value);
            }

            var properties = type.GetProperties();
            foreach (var p in properties)
            {
                if (!p.CanRead || !p.CanWrite || p.GetIndexParameters().Length > 0) { continue; }
                if (Attribute.IsDefined(p, typeof(DontSaveAttribute))) { continue; }

                var value = Get(p.PropertyType, p.Name, p.GetValue(returnThis));
                p.SetValue(returnThis, value);
            }

            return returnThis;
        }
    }
}