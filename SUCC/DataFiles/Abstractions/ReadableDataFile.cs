using SUCC.ParsingLogic;
using SUCC.MemoryFiles;
using System;
using System.Collections.Generic;

namespace SUCC.Abstractions
{
    /// <summary>
    /// A SUCC file that can be read from.
    /// </summary>
    public abstract class ReadableDataFile
    {
        internal List<Line> TopLevelLines { get; private set; } = new List<Line>();
        internal Dictionary<string, KeyNode> TopLevelNodes { get; private set; } = new Dictionary<string, KeyNode>();

        /// <summary>
        /// A quasi-unique string, to be used when you need to sort a bunch of DataFiles.
        /// </summary>
        public abstract string Identifier { get; }

        /// <summary>
        /// When a default value is not supplied in Get, we search for it in this.
        /// </summary>
        protected MemoryReadOnlyDataFile DefaultFileCache { get; }

        public ReadableDataFile(string defaultFileText = null)
        {
            if (defaultFileText == null)
                DefaultFileCache = null; // prevent infinite recursion lol
            else
                DefaultFileCache = new MemoryReadOnlyDataFile(defaultFileText, null);
        }

        /// <summary> Load the file text from wherever you're storing it </summary>
        protected abstract string GetSavedText();

        /// <summary> Reloads the data stored on disk into this object. </summary>
        public void ReloadAllData()
        {
            try
            {
                string succ = GetSavedText();

                var data = DataConverter.DataStructureFromSucc(succ, this);
                TopLevelLines = data.topLevelLines;
                TopLevelNodes = data.topLevelNodes;
            }
            catch (Exception e)
            {
                throw new Exception($"error parsing data from file: {e.Message}");
            }
        }

        /// <summary> Gets the data as it appears in file </summary>
        public string GetRawText()
            => DataConverter.SuccFromDataStructure(TopLevelLines);

        /// <summary> Gets the data as it appears in file, as an array of strings (one for each line) </summary>
        public string[] GetRawLines()
            => GetRawText().SplitIntoLines();

        /// <summary> Returns all top level keys in the file, in the order they appear in the file. </summary>
        public string[] GetTopLevelKeysInOrder()
        {
            var keys = new string[TopLevelNodes.Count];
            int count = 0;
            foreach (var line in TopLevelLines)
            {
                if (line is KeyNode node)
                {
                    keys[count] = node.Key;
                    count++;
                }
            }

            return keys;
        }

        /// <summary> This is faster than GetTopLevelKeysInOrder() but the keys may not be in the order they appear in the file </summary>
        public IReadOnlyCollection<string> TopLevelKeys
            => TopLevelNodes.Keys;

        /// <summary> Whether a top-level key exists in the file </summary>
        public bool KeyExists(string key)
            => TopLevelNodes.ContainsKey(key);

        /// <summary> Whether a key exists in the file at a nested path </summary>
        public bool KeyExistsAtPath(params string[] path)
        {
            if (path.Length < 1)
                throw new ArgumentException($"{nameof(path)} must have a length greater than 0");

            if (!KeyExists(path[0]))
                return false;

            var topNode = TopLevelNodes[path[0]];
            for (int i = 1; i < path.Length; i++)
            {
                if (topNode.ContainsChildNode(path[i]))
                    topNode = topNode.GetChildAddressedByName(path[i]);
                else
                    return false;
            }

            return true;
        }


        /// <summary> Like <see cref="Get{T}(string, T)"/>, but the default value is searched for in the default file text </summary>
        public T Get<T>(string key)
            => (T)GetNonGeneric(typeof(T), key);

        /// <summary> Like <see cref="GetNonGeneric(Type, string, object)"/>, but the default value is searched for in the default file text </summary>
        public object GetNonGeneric(Type type, string key)
        {
            var defaultDefaultValue = type.GetDefaultValue();
            object defaultValue = DefaultFileCache?.GetNonGeneric(type, key, defaultDefaultValue) ?? defaultDefaultValue;
            return this.GetNonGeneric(type, key, defaultValue);
        }

        // many of these methods are virtual so that their overrides in ReadableWritableDataFile
        // can have differing xml documentation.

        /// <summary> Get some data from the file, or return a default value if the data does not exist </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="defaultValue"> if the key does not exist in the file, this value is returned instead </param>
        public virtual T Get<T>(string key, T defaultValue)
            => (T)GetNonGeneric(typeof(T), key, defaultValue);

        /// <summary> Non-generic version of Get. You probably want to use Get. </summary>
        /// <param name="type"> the type to get the data as </param>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="defaultValue"> if the key does not exist in the file, this value is returned instead </param>
        public virtual object GetNonGeneric(Type type, string key, object defaultValue)
        {
            if (!KeyExists(key))
                return defaultValue;

            var node = TopLevelNodes[key];
            return NodeManager.GetNodeData(node, type);
        }

        /// <summary> Like <see cref="GetAtPath{T}(T, string[])"/>, but the default value is searched for in the default file text </summary>
        public T GetAtPath<T>(params string[] path)
            => (T)GetAtPathNonGeneric(typeof(T), path);

        /// <summary> Like <see cref="GetAtPathNonGeneric(Type, object, string[])"/>, but the value is searched for in the default file text </summary>
        public object GetAtPathNonGeneric(Type type, params string[] path)
        {
            var defaultDefaultValue = type.GetDefaultValue();
            object defaultValue = DefaultFileCache?.GetAtPathNonGeneric(type, defaultDefaultValue, path) ?? defaultDefaultValue;
            return this.GetAtPathNonGeneric(type, defaultValue, path);
        }

        /// <summary> 
        /// Like Get but works for nested paths instead of just the top level of the file 
        /// </summary>
        public virtual T GetAtPath<T>(T defaultValue, params string[] path)
            => (T)GetAtPathNonGeneric(typeof(T), defaultValue, path);

        /// <summary>
        /// Non-generic version of GetAtPath. You probably want to use GetAtPath.
        /// </summary>
        public virtual object GetAtPathNonGeneric(Type type, object defaultValue, params string[] path)
        {
            if (defaultValue != null && !type.IsAssignableFrom(defaultValue.GetType()))
                throw new InvalidCastException($"Expected type {type}, but the object is of type {defaultValue.GetType()}");

            if (!KeyExistsAtPath(path))
                return defaultValue;

            var topNode = TopLevelNodes[path[0]];
            for (int i = 1; i < path.Length; i++)
            {
                topNode = topNode.GetChildAddressedByName(path[i]);
            }

            return NodeManager.GetNodeData(topNode, type);
        }


        public bool TryGet<T>(string key, out T value)
        {
            if (!KeyExists(key))
            {
                value = default;
                return false;
            }

            value = Get<T>(key);
            return true;
        }


        /// <summary> Interpret this file as an object of type T, using that type's fields and properties as top-level keys. </summary>
        public T GetAsObject<T>() => (T)GetAsObjectNonGeneric(typeof(T));

        /// <summary> Non-generic version of GetAsObject. You probably wantto use GetAsObject. </summary>
        /// <param name="type"> the type to get this object as </param>
        public object GetAsObjectNonGeneric(Type type)
        {
            object returnThis = Activator.CreateInstance(type);

            foreach (var m in type.GetValidMembers())
            {
                var value = GetNonGeneric(m.MemberType, m.Name, m.GetValue(returnThis));
                m.SetValue(returnThis, value);
            }

            return returnThis;
        }

        /// <summary> Interpret this file as a dictionary. Top-level keys in the file are interpreted as keys in the dictionary. </summary>
        /// <remarks> TKey must be a Base Type </remarks>
        public Dictionary<TKey, TValue> GetAsDictionary<TKey, TValue>()
        {
            if (!BaseTypes.IsBaseType(typeof(TKey)))
                throw new Exception("When using GetAsDictionary, TKey must be a base type");

            var keys = this.TopLevelKeys;
            var dictionary = new Dictionary<TKey, TValue>(capacity: keys.Count);

            foreach (var keyText in keys)
            {
                TKey key = BaseTypes.ParseBaseType<TKey>(keyText);
                TValue value = NodeManager.GetNodeData<TValue>(TopLevelNodes[keyText]);
                dictionary.Add(key, value);
            }

            return dictionary;
        }
    }
}