using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SUCC.ParsingLogic
{
    // This class used to use dynamics instead of this non-generic generics trickery.
    // However, the old way only worked when compiled with Unity's compiler for Windows.
    // On other platforms, or when compiled by Visual Studio, they would throw RuntimeBinderExceptions.
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



        // arrays are the only type in here that doesn't require non-generic generics trickery
        private static void SetArrayNode(Node node, object array, Type arrayType, FileStyle style)
        {
            Type elementType = arrayType.GetElementType();

            var boi = (Array)array;

            node.CapChildCount(boi.Length);

            for (int i = 0; i < boi.Length; i++)
                NodeManager.SetNodeData(node.GetChildAddressedByListNumber(i), boi.GetValue(i), elementType, style);
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


        private static void SetListNode(Node node, object list, Type listType, FileStyle style)
        {
            Type elementType = listType.GetGenericArguments()[0];

            SetListNodeG.MakeGenericMethod(elementType)
                .Invoke(null, node, list, style);
        }
        private static MethodInfo SetListNodeG = GetMethod(nameof(SetListNodeGeneric));
        private static void SetListNodeGeneric<T>(Node node, List<T> list, FileStyle style)
        {
            node.CapChildCount(list.Count);

            for (int i = 0; i < list.Count; i++)
                NodeManager.SetNodeData<T>(node.GetChildAddressedByListNumber(i), list[i], style);
        }

        private static object RetrieveList(Node node, Type listType)
        {
            Type elementType = listType.GetGenericArguments()[0];

            return RetrieveListG.MakeGenericMethod(elementType)
                .Invoke(null, node);
        }
        private static MethodInfo RetrieveListG = GetMethod(nameof(RetrieveListGeneric));
        private static List<T> RetrieveListGeneric<T>(Node node)
        {
            var list = new List<T>(capacity: node.ChildNodes.Count);

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                ListNode child = node.GetChildAddressedByListNumber(i);
                var item = NodeManager.GetNodeData<T>(child);
                list.Add(item);
            }

            return list;
        }


        private static void SetHashSetNode(Node node, object hashset, Type hashsetType, FileStyle style)
        {
            Type elementType = hashsetType.GetGenericArguments()[0];

            SetHashSetNodeG.MakeGenericMethod(elementType)
                .Invoke(null, node, hashset, style);
        }
        private static MethodInfo SetHashSetNodeG = GetMethod(nameof(SetHashSetNodeGeneric));
        private static void SetHashSetNodeGeneric<T>(Node node, HashSet<T> hashset, FileStyle style)
        {
            node.CapChildCount(hashset.Count);

            int i = 0;
            foreach (var item in hashset)
            {
                NodeManager.SetNodeData<T>(node.GetChildAddressedByListNumber(i), item, style);
                i++;
            }
        }

        private static object RetrieveHashSet(Node node, Type hashsetType)
        {
            Type elementType = hashsetType.GetGenericArguments()[0];

            return RetrieveHashSetG.MakeGenericMethod(elementType)
                .Invoke(null, node);
        }
        private static MethodInfo RetrieveHashSetG = GetMethod(nameof(RetrieveHashSetGeneric));
        private static HashSet<T> RetrieveHashSetGeneric<T>(Node node)
        {
            var hashset = new HashSet<T>(); // todo: use the capacity constructor once unity can use .Net 4.7.2 - i.e. new HashSet<T>(capacity: node.ChildNodes.Count)

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                ListNode child = node.GetChildAddressedByListNumber(i);
                var item = NodeManager.GetNodeData<T>(child);
                hashset.Add(item);
            }

            return hashset;
        }


        private static void SetDictionaryNode(Node node, object dictionary, Type dictionaryType, FileStyle style, bool forceArrayMode = false)
        {
            Type keyType = dictionaryType.GetGenericArguments()[0];
            Type valueType = dictionaryType.GetGenericArguments()[1];

            SetDictionaryG.MakeGenericMethod(keyType, valueType)
                .Invoke(null, node, dictionary, style, forceArrayMode);
        }
        private static MethodInfo SetDictionaryG = GetMethod(nameof(SetDictionaryNodeGeneric));
        private static void SetDictionaryNodeGeneric<TKey, TValue>(Node node, Dictionary<TKey, TValue> dictionary, FileStyle style, bool forceArrayMode = false)
        {
            bool keyIsBase = BaseTypes.IsBaseType(typeof(TKey));

            if (keyIsBase && !forceArrayMode && !style.AlwaysArrayDictionaries)
            {
                // we might have switched between standard and array dictionary storage, and if so, children need to be reset
                if (node.ChildNodeType != NodeChildrenType.key)
                    node.ClearChildren(newChildrenType: NodeChildrenType.key);

                var CurrentKeys = new List<string>(capacity: dictionary.Count);
                foreach (var key in dictionary.Keys)
                {
                    var value = dictionary[key];

                    string keyAsText = BaseTypes.SerializeBaseType<TKey>(key, style);

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

        private static object RetrieveDictionary(Node node, Type dictionaryType)
        {
            Type keyType = dictionaryType.GetGenericArguments()[0];
            Type valueType = dictionaryType.GetGenericArguments()[1];

            return RetrieveDictionaryG.MakeGenericMethod(keyType, valueType)
                .Invoke(null, node);
        }
        private static MethodInfo RetrieveDictionaryG = GetMethod(nameof(RetrieveDictionaryGeneric));
        private static Dictionary<TKey, TValue> RetrieveDictionaryGeneric<TKey, TValue>(Node node)
        {
            bool keyIsBase = BaseTypes.IsBaseType(typeof(TKey));

            var dictionary = new Dictionary<TKey, TValue>(capacity: node.ChildNodes.Count);

            if (keyIsBase && node.ChildNodeType == NodeChildrenType.key)
            {
                foreach (var child in node.ChildNodes)
                {
                    string childKey = (child as KeyNode).Key;
                    var key = BaseTypes.ParseBaseType<TKey>(childKey);
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


        private static MethodInfo GetMethod(string methodName)
            => typeof(CollectionTypes).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
    }
}
