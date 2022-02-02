using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SUCC.ParsingLogic.CollectionTypes
{
    internal static class Dictionaries
    {
        public static bool IsDictionaryType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }


        private static MethodInfo GetMethod(string methodName)
            => typeof(Dictionaries).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);


        public static void SetDictionaryNode(Node node, object dictionary, Type dictionaryType, FileStyle style, bool forceArrayMode = false)
        {
            Type keyType = dictionaryType.GetGenericArguments()[0];
            Type valueType = dictionaryType.GetGenericArguments()[1];

            SetDictionary_.GetBoundGenericMethod(keyType, valueType)
                .Invoke(null, node, dictionary, style, forceArrayMode);
        }
        private static GenericMethodHelper_2 SetDictionary_ = new GenericMethodHelper_2(GetMethod(nameof(SetDictionaryNodeGeneric)));
        private static void SetDictionaryNodeGeneric<TKey, TValue>(Node node, Dictionary<TKey, TValue> dictionary, FileStyle style, bool forceArrayMode = false)
        {
            bool keyIsBase = BaseTypesManager.IsBaseType(typeof(TKey));

            if (keyIsBase && !forceArrayMode && !style.AlwaysArrayDictionaries)
            {
                // we might have switched between standard and array dictionary storage, and if so, children need to be reset
                if (node.ChildNodeType != NodeChildrenType.key)
                    node.ClearChildren(newChildrenType: NodeChildrenType.key);

                var CurrentKeys = new List<string>(capacity: dictionary.Count);
                foreach (var key in dictionary.Keys)
                {
                    var value = dictionary[key];

                    string keyAsText = BaseTypesManager.SerializeBaseType<TKey>(key, style);

                    if (!Utilities.IsValidKey(keyAsText))
                    {
                        SetDictionaryNodeGeneric(node, dictionary, style, forceArrayMode: true);
                        return;
                    }

                    CurrentKeys.Add(keyAsText);
                    KeyNode child = node.GetChildAddressedByName(keyAsText);
                    NodeManager.SetNodeData<TValue>(child, value, style);
                }

                // make sure that old data in the file is deleted when a new dictionary is saved.
                // node.ClearChildren() is not used because we want to keep comments and whitespace intact as much as possible.
                foreach (var key in node.GetChildKeys())
                {
                    if (!CurrentKeys.Contains(key))
                        node.RemoveChild(key);
                }
            }
            else // save dictionary as KeyValuePair<TKey, TValue>[]
            {
                // we might have switched between standard and array dictionary storage, and if so, children need to be reset
                if (node.ChildNodeType != NodeChildrenType.list)
                    node.ClearChildren(newChildrenType: NodeChildrenType.list);

                var array = GetWritableKeyValuePairArray(dictionary);
                NodeManager.SetNodeData(node, array, array.GetType(), style);
            }
        }


        public static object RetrieveDictionary(Node node, Type dictionaryType)
        {
            Type keyType = dictionaryType.GetGenericArguments()[0];
            Type valueType = dictionaryType.GetGenericArguments()[1];

            return RetrieveDictionary_.GetBoundGenericMethod(keyType, valueType)
                .Invoke(null, node);
        }
        private static GenericMethodHelper_2 RetrieveDictionary_ = new GenericMethodHelper_2(GetMethod(nameof(RetrieveDictionaryGeneric)));
        private static Dictionary<TKey, TValue> RetrieveDictionaryGeneric<TKey, TValue>(Node node)
        {
            bool keyIsBase = BaseTypesManager.IsBaseType(typeof(TKey));

            var dictionary = new Dictionary<TKey, TValue>(capacity: node.ChildNodes.Count);

            if (keyIsBase && node.ChildNodeType == NodeChildrenType.key)
            {
                foreach (var child in node.ChildNodes)
                {
                    string childKey = (child as KeyNode).Key;
                    var key = BaseTypesManager.ParseBaseType<TKey>(childKey);
                    var value = NodeManager.GetNodeData<TValue>(child);
                    dictionary.Add(key, value);
                }
            }
            else
            {
                var array = NodeManager.GetNodeData<WritableKeyValuePair<TKey, TValue>[]>(node);
                foreach (var kvp in array)
                    dictionary.Add(kvp.key, kvp.value);
            }

            return dictionary;
        }

        private static WritableKeyValuePair<TKey, TValue>[] GetWritableKeyValuePairArray<TKey, TValue>(Dictionary<TKey, TValue> boi)
        {
            var unwritable = Enumerable.ToArray(boi);
            var writable = new WritableKeyValuePair<TKey, TValue>[unwritable.Length];

            for (int i = 0; i < unwritable.Length; i++)
                writable[i] = new WritableKeyValuePair<TKey, TValue>(unwritable[i].Key, unwritable[i].Value);

            return writable;
        }

        private struct WritableKeyValuePair<TKey, TValue>
        {
            public WritableKeyValuePair(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }

            public TKey key;
            public TValue value;
        }
    }
}
