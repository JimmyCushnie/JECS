using System.Collections.Generic;
using System;
using System.Linq;

namespace SUCC.Types
{
    internal static class CollectionTypes
    {
        internal static bool TrySetCollection(Node node, object data, Type collectionType, FileStyle style)
        {
            if (collectionType.IsArray) SetArrayNode(node, data, collectionType, style);
            else if (collectionType.IsList()) SetListNode(node, data, collectionType, style);
            else if (collectionType.IsHashSet()) SetHashSetNode(node, data, collectionType, style);
            else if (collectionType.IsDictionary()) SetDictionaryNode(node, data, collectionType, style);

            else if (node.ChildNodeType == NodeChildrenType.list)
                throw new FormatException($"{collectionType} is not a supported collection type");
            else return false;

            return true;
        }

        internal static object TryGetCollection(Node node, Type collectionType)
        {
            object data = null;

            if (collectionType.IsArray) data = RetrieveArray(node, collectionType);
            else if (collectionType.IsList()) data = RetrieveList(node, collectionType);
            else if (collectionType.IsHashSet()) data = RetrieveHashSet(node, collectionType);
            else if (collectionType.IsDictionary()) data = RetrieveDictionary(node, collectionType);

            else if (node.ChildNodeType == NodeChildrenType.list)
                throw new FormatException($"{collectionType} is not a supported collection type");

            return data;
        }

        private static bool IsList(this Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

        private static bool IsHashSet(this Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>);

        private static bool IsDictionary(this Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);




        private static void SetArrayNode(Node node, dynamic array, Type arrayType, FileStyle style)
        {
            node.CapChildCount(array.Length); // prevent extra children from sticking around

            Type elementType = arrayType.GetElementType();
            for (int i = 0; i < array.Length; i++)
                NodeManager.SetNodeData(node.GetChildAddressedByListNumber(i), array[i], elementType, style);
        }

        private static object RetrieveArray(Node node, Type arrayType)
        {
            Type elementType = arrayType.GetElementType();
            var array = Array.CreateInstance(elementType, node.ChildNodes.Count);

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                ListNode child = node.GetChildAddressedByListNumber(i);
                object element = NodeManager.GetNodeData(child, elementType);
                array.SetValue(element, i);
            }

            return array;
        }


        private static void SetListNode(Node node, dynamic list, Type listType, FileStyle style)
        {
            node.CapChildCount(list.Count);

            Type elementType = listType.GetGenericArguments()[0];
            for (int i = 0; i < list.Count; i++)
                NodeManager.SetNodeData(node.GetChildAddressedByListNumber(i), list[i], elementType, style);
        }

        private static object RetrieveList(Node node, Type listType)
        {
            Type elementType = listType.GetGenericArguments()[0];
            dynamic list = Activator.CreateInstance(listType, node.ChildNodes.Count);

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                ListNode child = node.GetChildAddressedByListNumber(i);
                dynamic item = NodeManager.GetNodeData(child, elementType);
                list.Add(item);
            }

            return list;
        }


        private static void SetHashSetNode(Node node, dynamic hashset, Type hashSetType, FileStyle style)
        {
            node.CapChildCount(hashset.Count);

            int i = 0;
            Type elementType = hashSetType.GetGenericArguments()[0];
            foreach (var item in hashset)
            {
                NodeManager.SetNodeData(node.GetChildAddressedByListNumber(i), item, elementType, style);
                i++;
            }
        }

        private static object RetrieveHashSet(Node node, Type hashSetType)
        {
            Type elementType = hashSetType.GetGenericArguments()[0];
            dynamic hashset = Activator.CreateInstance(hashSetType); // todo: use the capacity constructor once unity can use .Net 4.7.2 - i.e. CreateInstance(hashSetType, node.ChildNodes.Count)

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                ListNode child = node.GetChildAddressedByListNumber(i);
                dynamic item = NodeManager.GetNodeData(child, elementType);
                hashset.Add(item);
            }

            return hashset;
        }


        private static void SetDictionaryNode(Node node, dynamic dictionary, Type dictionaryType, FileStyle style, bool forceArrayMode = false)
        {
            Type keyType = dictionaryType.GetGenericArguments()[0];
            Type valueType = dictionaryType.GetGenericArguments()[1];
            bool keyIsBase = BaseTypes.IsBaseType(keyType);

            if (keyIsBase && !forceArrayMode && !style.AlwaysArrayDictionaries)
            {
                // we might have switched between standard and array dictionary storage, and if so, children need to be reset
                if (node.ChildNodeType != NodeChildrenType.key)
                    node.ClearChildren(newChildrenType: NodeChildrenType.key);

                var CurrentKeys = new List<string>(capacity: dictionary.Count);
                foreach (var key in dictionary.Keys)
                {
                    var value = dictionary[key];

                    string keyAsText = BaseTypes.SerializeBaseType(key, keyType, style);

                    if (!Utilities.IsValidKey(keyAsText))
                    {
                        SetDictionaryNode(node, dictionary, dictionaryType, style, forceArrayMode: true);
                        return;
                    }

                    CurrentKeys.Add(keyAsText);
                    KeyNode child = node.GetChildAddressedByName(keyAsText);
                    NodeManager.SetNodeData(child, value, valueType, style);
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

        private static object RetrieveDictionary(Node node, Type dictionaryType)
        {
            Type keyType = dictionaryType.GetGenericArguments()[0];
            Type valueType = dictionaryType.GetGenericArguments()[1];
            bool keyIsBase = BaseTypes.IsBaseType(keyType);

            dynamic dictionary = Activator.CreateInstance(dictionaryType);

            if (keyIsBase && node.ChildNodeType == NodeChildrenType.key)
            {
                foreach (var child in node.ChildNodes)
                {
                    string childKey = (child as KeyNode).Key;
                    dynamic key = BaseTypes.ParseBaseType(childKey, keyType);
                    dynamic value = NodeManager.GetNodeData(child, valueType);
                    dictionary.Add(key, value);
                }
            }
            else
            {
                // treat it as a WritableKeyValuePair<keyType, valueType>[]
                var type = GetWritableKeyValuePairArray(dictionary).GetType();
                dynamic array = NodeManager.GetNodeData(node, type);
                foreach (var kvp in array)
                    dictionary.Add(kvp.Key, kvp.Value);
            }

            return dictionary;
        }

        private static WritableKeyValuePair<TKey, TValue>[] GetWritableKeyValuePairArray<TKey, TValue>(Dictionary<TKey, TValue> boi)
        {
            var unwritable = Enumerable.ToArray(boi);
            var writable = new WritableKeyValuePair<TKey, TValue>[unwritable.Length];

            for(int i = 0; i < unwritable.Length; i++)
                writable[i] = new WritableKeyValuePair<TKey, TValue>(unwritable[i].Key, unwritable[i].Value);

            return writable;
        }

        private struct WritableKeyValuePair<TKey, TValue>
        {
            public WritableKeyValuePair(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }

            public TKey Key { get; set; }
            public TValue Value { get; set; }
        }
    }
}
