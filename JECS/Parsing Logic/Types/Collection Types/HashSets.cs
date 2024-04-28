using System;
using System.Collections.Generic;
using System.Reflection;

namespace JECS.ParsingLogic.CollectionTypes
{
    internal static class HashSets
    {
        public static bool IsHashSetType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>);
        }


        private static MethodInfo GetMethod(string methodName)
            => typeof(HashSets).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);


        public static void SetHashSetNode(Node node, object hashset, Type hashsetType, FileStyle style)
        {
            Type elementType = hashsetType.GetGenericArguments()[0];

            SetHashSetNode_.GetBoundGenericMethod(elementType)
                .Invoke(null, node, hashset, style);
        }
        private static GenericMethodHelper SetHashSetNode_ = new GenericMethodHelper(GetMethod(nameof(SetHashSetNodeGeneric)));
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

        public static object RetrieveHashSet(Node node, Type hashsetType)
        {
            Type elementType = hashsetType.GetGenericArguments()[0];

            return RetrieveHashSet_.GetBoundGenericMethod(elementType)
                .Invoke(null, node);
        }
        private static GenericMethodHelper RetrieveHashSet_ = new GenericMethodHelper(GetMethod(nameof(RetrieveHashSetGeneric)));
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
    }
}
