using System;

namespace SUCC.InternalParsingLogic
{
    /// <summary>
    /// Gets and sets the data encoded by Nodes.
    /// </summary>
    internal static class NodeManager
    {
        internal static void SetNodeData<T>(Node node, T data, FileStyle style) => SetNodeData(node, data, typeof(T), style);
        internal static void SetNodeData(Node node, object data, Type type, FileStyle style)
        {
            if (data == null)
                throw new Exception("you can't serialize null");

            // ensure the type is initialized. This is especially important if it's added as
            // a base type in the type's static constructor.
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);

            string dataAsString = data as string;
            if (type == typeof(string) && (dataAsString.ContainsNewLine() || node.ChildNodes.Count > 0))
                BaseTypes.SetStringSpecialCase(node, dataAsString, style);

            else if (BaseTypes.IsBaseType(type))
                BaseTypes.SetBaseTypeNode(node, data, type, style);

            else if (CollectionTypes.TrySetCollection(node, data, type, style))
                return;

            else
                ComplexTypes.SetComplexNode(node, data, type, style);
        }

        internal static T GetNodeData<T>(Node node) => (T)GetNodeData(node, typeof(T));
        internal static object GetNodeData(Node node, Type type)
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);

            try
            {
                if (type == typeof(string) && node.Value == MultiLineStringNode.Terminator && node.ChildLines.Count > 0)
                    return BaseTypes.ParseSpecialStringCase(node);

                if (BaseTypes.IsBaseType(type))
                    return BaseTypes.ParseBaseType(node.Value, type);

                var collection = CollectionTypes.TryGetCollection(node, type);
                if (collection != null) return collection;

                if (!String.IsNullOrEmpty(node.Value))
                    return ComplexTypeShortcuts.GetFromShortcut(node.Value, type);

                return ComplexTypes.RetrieveComplexType(node, type);
            }
            catch (Exception e)
            {
                throw new Exception($"Error getting data of type {type} from node: {e.InnerException}");
            }
        }
    }
}