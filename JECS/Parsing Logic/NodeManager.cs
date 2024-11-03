using JECS.ParsingLogic.CollectionTypes;
using System;

namespace JECS.ParsingLogic
{
    /// <summary>
    /// Gets and sets the data encoded by Nodes.
    /// </summary>
    internal static class NodeManager
    {
        internal static void SetNodeData<T>(Node node, T data, FileStyle style) => SetNodeData(node, data, typeof(T), style);
        internal static void SetNodeData(Node node, object data, Type type, FileStyle style)
        {
            if (data == null && !type.IsNullableType())
                throw new Exception($"There's been some kind of coding mistake: {nameof(data)} is null, but type {type} is not nullable.");
            
            if (data != null && !type.IsAssignableFrom(data.GetType()))
                throw new Exception($"There's been some kind of coding mistake: {nameof(data)} is of type {data.GetType()}, which is not assignable to type {type}.");



            if (data == null)
            {
                node.ClearChildren();
                node.Value = Utilities.NullIndicator;
                return;
            }

            if (node.Value == Utilities.NullIndicator)
            {
                node.ClearValue();
            }

            var underlyingNullableType = Nullable.GetUnderlyingType(type);
            if (underlyingNullableType != null)
                type = underlyingNullableType;

            // Ensure the type is initialized. This is especially important if it's added as
            // a base type in the type's static constructor.
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);

            // If we try to save a single-line string and find it is currently saved as a multi-line string, we do NOT remove the multi-line formatting.
            // The reason for this is that there might be comments on the """s, and we want to preserve those comments.
            // Also, this happens in only two cases:
            //     1. A string that is usually single-line is manually switched to multi-line formatting by a user
            //     2. A string is saved as multi-line, then later saved as single-line
            // In case 1, we don't want to piss off the user; keep it how they like it.
            // In case 2, the string is probably going to be saved again later with multiple lines. It doesn't seem necessary to disrupt the structure
            // of the file for something temporary.
            string dataAsString = data as string;
            if (type == typeof(string) && (dataAsString.ContainsNewLine() || node.ChildNodes.Count > 0))
            {
                MultiLineStringSpecialCaseHandler.SetStringSpecialCase(node, dataAsString, style);
                return;
            }

            if (BaseTypesManager.IsBaseType(type))
            {
                SetBaseTypeNode(node, data, type, style);
                return;
            }

            if (CollectionTypesManager.IsSupportedType(type))
            {
                CollectionTypesManager.SetCollectionNode(node, data, type, style);
                return;
            }

            ComplexTypes.SetComplexNode(node, data, type, style);
        }

        internal static T GetNodeData<T>(Node node) => (T)GetNodeData(node, typeof(T));
        internal static object GetNodeData(Node node, Type type)
        {
            if (node.Value == Utilities.NullIndicator)
                return null;

            var underlyingNullableType = Nullable.GetUnderlyingType(type);
            if (underlyingNullableType != null)
                type = underlyingNullableType;


            // Ensures that the type's static constructor has been run before we try to load it.
            // A convenient place to add base type rules is in the type's static constructor, so
            // this ensures the base type rules are registered before they are needed.
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);

            if (type == typeof(string) && node.Value == MultiLineStringNode.Terminator && node.ChildNodeType == NodeChildrenType.MultiLineString && node.ChildLines.Count > 0)
                return MultiLineStringSpecialCaseHandler.ParseSpecialStringCase(node);

            if (BaseTypesManager.IsBaseType(type))
                return RetrieveDataWithErrorChecking(() => RetrieveBaseTypeNode(node, type));

            if (CollectionTypesManager.IsSupportedType(type))
                return RetrieveDataWithErrorChecking(() => CollectionTypesManager.RetrieveCollection(node, type));

            if (node.HasValue)
                return RetrieveDataWithErrorChecking(() => ComplexTypeShortcuts.GetFromShortcut(node.Value, type));

            return RetrieveDataWithErrorChecking(() => ComplexTypes.RetrieveComplexType(node, type));


            object RetrieveDataWithErrorChecking(Func<object> retrieveDataFunction)
            {
                try
                {
                    return retrieveDataFunction.Invoke();
                }
                catch (CannotRetrieveDataFromNodeException deeperException)
                {
                    // If there's a parsing error deeper in the tree, we want to throw *that* error, so the user gets a line
                    // number appropriate to the actual error.
                    throw deeperException;
                }
                // catch
                // {
                //     throw new CannotRetrieveDataFromNodeException(node, type);
                // }
            }
        }


        private static void SetBaseTypeNode(Node node, object data, Type type, FileStyle style)
        {
            node.ClearChildren();
            node.Value = BaseTypesManager.SerializeBaseType(data, type, style);
        }

        private static object RetrieveBaseTypeNode(Node node, Type type)
        {
            // Base types are unique in that they CAN be serialized as a single line, and indeed that is how JECS will always save them.
            // However, you CAN manually write a file that uses complex type rules for a base type, and thanks to the logic in this method,
            // it will still work.
            // See https://github.com/JimmyCushnie/JECS/issues/26

            if (node.ChildNodeType == NodeChildrenType.Key && node.ChildNodes.Count > 0)
                return ComplexTypes.RetrieveComplexType(node, type);

            if (BaseTypesManager.TryParseBaseType(node.Value, type, out var result))
                return result;

            return ComplexTypeShortcuts.GetFromShortcut(node.Value, type);
        }
    }
}
