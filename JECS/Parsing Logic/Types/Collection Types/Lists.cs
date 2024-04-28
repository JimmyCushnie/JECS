using System;
using System.Collections.Generic;
using System.Reflection;

namespace JECS.ParsingLogic.CollectionTypes
{
    internal static class Lists
    {
        public static bool IsListType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }


        private static MethodInfo GetMethod(string methodName)
            => typeof(Lists).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);


        public static void SetListNode(Node node, object list, Type listType, FileStyle style)
        {
            Type elementType = listType.GetGenericArguments()[0];

            SetListNode_.GetBoundGenericMethod(elementType)
                .Invoke(null, node, list, style);
        }
        private static GenericMethodHelper SetListNode_ = new GenericMethodHelper(GetMethod(nameof(SetListNodeGeneric)));
        private static void SetListNodeGeneric<T>(Node node, List<T> list, FileStyle style)
        {
            node.CapChildCount(list.Count);

            for (int i = 0; i < list.Count; i++)
                NodeManager.SetNodeData<T>(node.GetChildAddressedByListNumber(i), list[i], style);
        }

        public static object RetrieveList(Node node, Type listType)
        {
            Type elementType = listType.GetGenericArguments()[0];

            return RetrieveList_.GetBoundGenericMethod(elementType)
                .Invoke(null, node);
        }
        private static GenericMethodHelper RetrieveList_ = new GenericMethodHelper(GetMethod(nameof(RetrieveListGeneric)));
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
    }
}
