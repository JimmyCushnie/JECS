using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;

namespace SUCC.Types
{
    internal static class CollectionTypes
    {
        internal static bool TrySetCollection(Node node, object data, Type collectionType)
        {
            if (collectionType.IsArray) SetArrayNode(node, data, collectionType);
            else if (collectionType.IsList()) SetListNode(node, data, collectionType);
            else if (collectionType.IsHashSet()) SetHashSetNode(node, data, collectionType);
            else if (collectionType.IsDictionary()) SetDictionaryNode(node, data, collectionType);

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




        private static void SetArrayNode(Node node, dynamic array, Type arrayType)
        {
            Type elementType = arrayType.GetElementType();
            for (int i = 0; i < array.Length; i++)
                NodeManager.SetNodeData(node.GetChildAddressedByListNumber(i), array[i], elementType);
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


        private static void SetListNode(Node node, dynamic list, Type listType)
        {
            Type elementType = listType.GetGenericArguments()[0];
            for (int i = 0; i < list.Count; i++)
                NodeManager.SetNodeData(node.GetChildAddressedByListNumber(i), list[i], elementType);
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


        private static void SetHashSetNode(Node node, dynamic hashset, Type hashSetType)
        {
            Type elementType = hashSetType.GetGenericArguments()[0];

            int i = 0;
            foreach (var item in hashset)
            {
                NodeManager.SetNodeData(node.GetChildAddressedByListNumber(i), item, elementType);
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


        private static void SetDictionaryNode(Node node, dynamic dictionary, Type dictionaryType)
        {
            Type keyType = dictionaryType.GetGenericArguments()[0];
            Type valueType = dictionaryType.GetGenericArguments()[1];
            bool keyIsBase = BaseTypes.IsBaseType(keyType);

            if (keyIsBase)
            {
                foreach (var key in dictionary.Keys)
                {
                    var value = dictionary[key];

                    KeyNode child = node.GetChildAddressedByName(BaseTypes.SerializeBaseType(key, keyType));
                    NodeManager.SetNodeData(child, value, valueType);
                }
            }
            else
            {
                // treat it as a KeyValuePair<keyType, valueType>[]
                var array = Enumerable.ToArray(dictionary);
                NodeManager.SetNodeData(node, array, array.GetType());
            }
        }

        private static object RetrieveDictionary(Node node, Type dictionaryType)
        {
            Type keyType = dictionaryType.GetGenericArguments()[0];
            Type valueType = dictionaryType.GetGenericArguments()[1];
            bool keyIsBase = BaseTypes.IsBaseType(keyType);

            dynamic dictionary = Activator.CreateInstance(dictionaryType);

            if (keyIsBase)
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
                // treat it as a KeyValuePair<keyType, valueType>[]
                dynamic array = NodeManager.GetNodeData(node, Enumerable.ToArray(dictionary).GetType());
                foreach (var kvp in array)
                    dictionary.Add(kvp.Key, kvp.Value);
            }

            return dictionary;
        }
    }
}
