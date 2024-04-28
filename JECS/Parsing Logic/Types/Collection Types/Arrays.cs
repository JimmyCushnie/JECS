using System;

namespace JECS.ParsingLogic.CollectionTypes
{
    internal static class Arrays
    {
        public static bool IsArrayType(Type type)
            => type.IsArray;


        public static void SetArrayNode(Node node, object array, Type arrayType, FileStyle style)
        {
            Type elementType = arrayType.GetElementType();

            var boi = (Array)array;

            node.CapChildCount(boi.Length);

            for (int i = 0; i < boi.Length; i++)
                NodeManager.SetNodeData(node.GetChildAddressedByListNumber(i), boi.GetValue(i), elementType, style);
        }

        public static object RetrieveArray(Node node, Type arrayType)
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
    }
}
