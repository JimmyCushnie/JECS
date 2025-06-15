using System;
using System.Collections.Generic;
using JECS.MemoryFiles;
using JECS.ParsingLogic;

namespace JECS.Abstractions
{
    /// <summary>
    /// A JECS file that can be read from.
    /// </summary>
    public abstract class ReadableDataFile
    {
        internal List<Line> TopLevelLines { get; private set; } = new List<Line>();
        internal Dictionary<string, KeyNode> TopLevelNodes { get; private set; } = new Dictionary<string, KeyNode>();

        /// <summary>
        /// A quasi-unique string, to be used when you need to sort a bunch of DataFiles.
        /// </summary>
        public abstract string Identifier { get; }

        public override string ToString() => Identifier;

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
            string jecs = GetSavedText();
            (TopLevelLines, TopLevelNodes) = DataConverter.DataStructureFromJecs(jecs, this);
        }

        /// <summary> Gets the data as it appears in file </summary>
        public string GetRawText()
            => DataConverter.JecsFromDataStructure(TopLevelLines);

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


        protected static void EnsureValueIsCorrectType(Type type, object defaultValue)
        {
            if (defaultValue != null && !type.IsAssignableFrom(defaultValue.GetType()))
                throw new InvalidCastException($"Expected type {type}, but the object is of type {defaultValue.GetType()}");
        }

        /// <summary> Tries to get a node for key from file. </summary>
        /// <param name="key"> The key of the top-level node in the file. </param>
        /// <param name="node"> The node for key, or <see langword="null"/> if not found. </param>
        /// <returns> <see langword="true"/>, if the node exists <see langword="false"/> otherwise. </returns>
        internal bool TryGetNode(string key, out KeyNode node)
            => TopLevelNodes.TryGetValue(key, out node);

        /// <summary> Tries to get a node at a path from file. </summary>
        /// <param name="topNode"> The node at path, or <see langword="null"/> if not found. </param>
        /// <param name="path"> The path to the node in file. </param>
        /// <returns> <see langword="true"/>, if the node exists <see langword="false"/> otherwise. </returns>
        /// <exception cref="ArgumentException"> When the path has a length smaller 1. </exception>
        internal bool TryGetNodeAtPath(out KeyNode topNode, params string[] path)
        {
            if (path.Length < 1)
                throw new ArgumentException($"{nameof(path)} must have a length greater than 0");

            if (!TryGetNode(path[0], out topNode))
            {
                topNode = null;
                return false;
            }

            for (int i = 1; i < path.Length; i++)
            {
                if (!topNode.TryGetChildAddressedByName(path[i], out topNode))
                {
                    topNode = null;
                    return false;
                }
            }

            return true;
        }


        /// <summary> Whether a top-level key exists in the file </summary>
        public bool KeyExists(string key)
            => TopLevelNodes.ContainsKey(key);

        /// <summary> Whether a key exists in the file at a nested path </summary>
        public bool KeyExistsAtPath(params string[] path)
            => TryGetNodeAtPath(out _, path);


        /// <summary> Tries to get value from file. </summary>
        /// <param name="key"> The key to the top-level value in the file. </param>
        /// <param name="value"> The value of type <typeparamref name="T"/>, if the value at <paramref name="key"/> was found else <see langword="null"/>. </param>
        /// <typeparam name="T"> The type which the value is expected to be. </typeparam>
        /// <returns> <see langword="true"/>, if the value exists <see langword="false"/> otherwise. </returns>
        public bool TryGet<T>(string key, out T value)
        {
            if (TryGetNonGeneric(typeof(T), key, out var objValue))
            {
                value = (T)objValue;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary> Tries to get value from file. </summary>
        /// <param name="type"> The type which the value is expected to be. </param>
        /// <param name="key"> The key to the top-level value in the file. </param>
        /// <param name="value"> The value of type <paramref name="type"/>, if the value at <paramref name="key"/> was found else <see langword="null"/>. </param>
        /// <returns> <see langword="true"/>, if the value exists <see langword="false"/> otherwise. </returns>
        public bool TryGetNonGeneric(Type type, string key, out object value)
        {
            if (!TryGetNode(key, out var node))
            {
                value = null;
                return false;
            }

            value = NodeManager.GetNodeData(node, type);
            return true;
        }

        // many of these methods are virtual so that their overrides in ReadableWritableDataFile
        // can have differing xml documentation.

        /// <summary> Like <see cref="Get{T}(string, T)"/>, but the default value is searched for in the default file text </summary>
        public virtual T Get<T>(string key)
            => (T)GetNonGeneric(typeof(T), key);

        /// <summary> Like <see cref="GetNonGeneric(Type, string, object)"/>, but the default value is searched for in the default file text </summary>
        public virtual object GetNonGeneric(Type type, string key)
        {
            if (TryGetNonGeneric(type, key, out var dataValue))
                return dataValue;

            if (DefaultFileCache != null && DefaultFileCache.TryGetNonGeneric(type, key, out var fallbackValue))
                return fallbackValue;

            return type.GetDefaultValue();
        }

        /// <summary> Get some data from the file, or return a default value if the data does not exist </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="defaultValue"> if the key does not exist in the file, this value is returned instead </param>
        public virtual T Get<T>(string key, T defaultValue)
            => TryGetNonGeneric(typeof(T), key, out var result) ? (T)result : defaultValue;

        /// <summary> Non-generic version of Get. You probably want to use Get. </summary>
        /// <param name="type"> the type to get the data as </param>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="defaultValue"> if the key does not exist in the file, this value is returned instead </param>
        public virtual object GetNonGeneric(Type type, string key, object defaultValue)
        {
            EnsureValueIsCorrectType(type, defaultValue);
            return TryGetNonGeneric(type, key, out var result) ? result : defaultValue;
        }


        /// <summary> Tries to get value from file. </summary>
        /// <param name="value"> The value of type <typeparamref name="T"/>, if the value at <paramref name="path"/> was found else <see langword="null"/>. </param>
        /// <param name="path"> The path to the value in the file. </param>
        /// <typeparam name="T"> The type which the value is expected to be. </typeparam>
        /// <returns> <see langword="true"/>, if the value exists <see langword="false"/> otherwise. </returns>
        public bool TryGetAtPath<T>(out T value, params string[] path)
        {
            if (TryGetAtPathNonGeneric(typeof(T), out var objValue, path))
            {
                value = (T)objValue;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary> Tries to get value from file. </summary>
        /// <param name="type"> The type which the value is expected to be. </param>
        /// <param name="value"> The value of type <paramref name="type"/>, if the value at <paramref name="path"/> was found else <see langword="null"/>. </param>
        /// <param name="path"> The path to the value in the file. </param>
        /// <returns> <see langword="true"/>, if the value exists <see langword="false"/> otherwise. </returns>
        public bool TryGetAtPathNonGeneric(Type type, out object value, params string[] path)
        {
            if (!TryGetNodeAtPath(out var node, path))
            {
                value = null;
                return false;
            }

            value = NodeManager.GetNodeData(node, type);
            return true;
        }

        /// <summary> Like <see cref="Get{T}(string)"/> but works for nested paths instead of just the top level of the file. </summary>
        public virtual T GetAtPath<T>(params string[] path)
            => (T)GetAtPathNonGeneric(typeof(T), path);

        /// <summary> Like <see cref="GetAtPathNonGeneric(Type, object, string[])"/>, but the value is searched for in the default file text </summary>
        public virtual object GetAtPathNonGeneric(Type type, params string[] path)
        {
            if (TryGetAtPathNonGeneric(type, out var dataValue, path))
                return dataValue;

            if (DefaultFileCache != null && DefaultFileCache.TryGetAtPathNonGeneric(type, out var fallbackValue, path))
                return fallbackValue;

            return type.GetDefaultValue();
        }

        /// <summary> Like <see cref="GetAtPath{T}(T, string[])"/>, but the default value is searched for in the default file text </summary>
        public virtual T GetAtPath<T>(T defaultValue, params string[] path)
            => TryGetAtPathNonGeneric(typeof(T), out var result, path) ? (T)result : defaultValue;

        /// <summary> Non-generic version of <see cref="GetAtPath{T}(T,string[])"/>. You probably want to use that one.</summary>
        public virtual object GetAtPathNonGeneric(Type type, object defaultValue, params string[] path)
        {
            EnsureValueIsCorrectType(type, defaultValue);
            return TryGetAtPathNonGeneric(type, out var result, path) ? result : defaultValue;
        }
    }
}