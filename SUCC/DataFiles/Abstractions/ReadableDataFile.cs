using SUCC.InternalParsingLogic;
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

        /// <summary> Load the file text from wherever you're storing it </summary>
        protected abstract string GetSavedText();

        /// <summary> Reloads the data stored on disk into this object. </summary>
        public void ReloadAllData()
        {
            try
            {
                string succ = GetSavedText();

                var data = DataConverter.DataStructureFromSUCC(succ, this);
                TopLevelLines = data.Item1;
                TopLevelNodes = data.Item2;
            }
            catch (Exception e)
            {
                throw new Exception($"error parsing data from file: {e.Message}");
            }
        }

        /// <summary> gets the data as it appears in file </summary>
        public string GetRawText()
            => DataConverter.SUCCFromDataStructure(TopLevelLines);

        /// <summary> gets the data as it appears in file, as an array of strings (one for each line) </summary>
        public string[] GetRawLines()
            => GetRawText().SplitIntoLines();

        /// <summary> returns all top level keys in the file, in the order they appear in the file. </summary>
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

        /// <summary> this is faster than GetTopLevelKeysInOrder() but the keys may not be in the order they appear in the file </summary>
        public IReadOnlyCollection<string> TopLevelKeys
            => TopLevelNodes.Keys;

        /// <summary> whether a top-level key exists in the file </summary>
        public bool KeyExists(string key)
            => TopLevelNodes.ContainsKey(key);

        /// <summary> whether a key exists in the file at a nested path </summary>
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

        // many of these methods are virtual so that their overrides in WritableDataFile
        // can have differing xml documentation.

        /// <summary> Get some data from the file, or return a default value if the data does not exist </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="defaultValue"> if the key does not exist in the file, this value is returned instead </param>
        public virtual T Get<T>(string key, T defaultValue = default)
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
            if (!KeyExistsAtPath(path))
                throw new Exception($"The specified path doesn't exist. Check {nameof(KeyExistsAtPath)} first.");

            var topNode = TopLevelNodes[path[0]];
            for (int i = 1; i < path.Length; i++)
            {
                topNode = topNode.GetChildAddressedByName(path[i]);
            }

            return NodeManager.GetNodeData(topNode, type);
        }


        /// <summary> Interpret this file as an object of type T, using that type's fields and properties as top-level keys. </summary>
        public T GetAsObject<T>() => (T)GetAsObjectNonGeneric(typeof(T));

        /// <summary> Non-generic version of GetAsObject. You probably wantto use GetAsObject. </summary>
        /// <param name="type"> the type to get this object as </param>
        public object GetAsObjectNonGeneric(Type type)
        {
            object returnThis = Activator.CreateInstance(type);

            foreach (var f in ComplexTypes.GetValidFields(type))
            {
                var value = GetNonGeneric(f.FieldType, f.Name, f.GetValue(returnThis));
                f.SetValue(returnThis, value);
            }

            foreach (var p in ComplexTypes.GetValidProperties(type))
            {
                var value = GetNonGeneric(p.PropertyType, p.Name, p.GetValue(returnThis));
                p.SetValue(returnThis, value);
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