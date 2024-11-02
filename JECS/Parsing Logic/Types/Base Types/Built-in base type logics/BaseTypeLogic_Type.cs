using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JECS.BuiltInBaseTypeLogics
{
    internal class BaseTypeLogic_Type : BaseTypeLogic<Type>
    {
        // Parsing types is pretty slow, so we cache the results
        private static Dictionary<string, Type> TypeCache { get; } = new Dictionary<string, Type>();
        public override Type ParseItem(string text)
        {
            if (TypeCache.TryGetValue(text, out Type type))
                return type;

            type = TypeStrings.ParseCsharpTypeName(text);
            TypeCache.Add(text, type);
            return type;
        }

        // Todo: should get my two-way dictionary class in here so the caching can work for serialization as well
        public override string SerializeItem(Type value)
        {
            return TypeStrings.GetCsharpTypeName(value);
        }
    }


    /// <summary>
    /// Turn C# type variables into and out of strings that look like C# code.
    /// </summary>
    internal class TypeStrings
    {
        // The purpose of this class is to work with strings representing types that look exactly like type declarations do in C# code.
        // Type.FullName differs from the type declarations in source code in a number of ways, all of which we have to account for. These ways are:
        // - Unbound generics look like System.Collections.Generic.List`1 instead of System.Collections.Generic.List<>
        // - Bound generics look like System.Collections.Generic.List`1[[System.Int32, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]] instead of System.Collections.Generic.List<System.Int32>
        // - Nested types look like Namespace.ParentType+NestedType instead of Namespace.ParentType.NestedType
        // The above points are all handled by this code. However, the below points are, for now at least, not handled:
        // - We don't save tuples like (Type1, Type2); instead we let it get saved as System.Tuple<Type1, Type2>
        // - We don't save nullables like Type?; instead we let it get saved as System.Nullable<Type>

        // Note to self: if I update this, I should also update the gist.
        // https://gist.github.com/JimmyCushnie/950e09d9f96fab16d7d723c5b1583c3a

        public static string GetCsharpTypeName(Type type)
        {
            string typeFullName = type.FullName;

            // You get this when you're recursively getting the generic parameters of an unbound generic type
            if (String.IsNullOrEmpty(typeFullName))
                return typeFullName;

            if (type.IsArray)
            {
                int lastLeftBracketIndex = typeFullName.LastIndexOf('[');
                string arrayDeclaration = typeFullName.Substring(lastLeftBracketIndex);

                return GetCsharpTypeName(type.GetElementType()) + arrayDeclaration;
            }

            // Fix the syntax of nested types
            typeFullName = typeFullName.Replace('+', '.');

            if (type.IsGenericType)
            {
                string typeNameWithoutGenerics = typeFullName.Split('`')[0];
                string genericArguments = String.Join(", ", type.GetGenericArguments().Select(GetCsharpTypeName));

                return typeNameWithoutGenerics + "<" + genericArguments + ">";
            }
            else
            {
                return typeFullName;
            }
        }

        public static Type ParseCsharpTypeName(string typeName)
        {
            if (!typeName.Contains('<'))
                return ParseTypeFromFullName(typeName);

            if (typeName.EndsWith("]"))
            {
                // Type is an array type
                int lastLeftBracketIndex = typeName.LastIndexOf('[');
                string arrayDeclaration = typeName.Substring(lastLeftBracketIndex);
                int arrayDimensions = arrayDeclaration.Count(c => c == ',') + 1;

                string elementTypeDeclaration = typeName.Substring(0, lastLeftBracketIndex);
                Type elementType = ParseCsharpTypeName(elementTypeDeclaration);

                if (arrayDimensions == 1)
                    return elementType.MakeArrayType();
                else
                    return elementType.MakeArrayType(arrayDimensions);
            }

            // Type is generic. We need to parse its generic parameters in a recursive way.

            var typeStack = new Stack<ParsingType>();

            typeStack.Push(new ParsingType());
            int previousTypeNameStartIndex = 0;

            bool withinArrayDeclaration = false;
            for (int i = 0; i < typeName.Length; i++)
            {
                switch (typeName[i])
                {
                    case '<':
                        typeNameFinished();
                        addGenericParameterToStack();
                        break;

                    case '>':
                        typeNameFinished();
                        typeStack.Pop();
                        break;

                    case ',':
                        if (withinArrayDeclaration) // For example: List<int[,,]>
                            break;

                        typeNameFinished();
                        typeStack.Pop();

                        addGenericParameterToStack();
                        break;

                    case '[':
                        withinArrayDeclaration = true;
                        break;

                    case ']':
                        withinArrayDeclaration = false;
                        break;
                }

                void typeNameFinished()
                {
                    var subTypeName =
                        typeName.Substring(previousTypeNameStartIndex, i - previousTypeNameStartIndex)
                        .Trim('<', '>', ',', ' ');

                    if (subTypeName.Length > 0)
                        typeStack.Peek().TypeFullName = subTypeName;

                    previousTypeNameStartIndex = i;
                }
                void addGenericParameterToStack()
                {
                    var genericParameter = new ParsingType();
                    typeStack.Peek().GenericParameters.Add(genericParameter);

                    typeStack.Push(genericParameter);
                }
            }

            if (typeStack.Count != 1)
                throw new Exception("Oh no, looks like the type name had some non-matching brackets");

            return typeStack.Pop().TurnIntoType();
        }

        private static Type ParseTypeFromFullName(string fullName)
        {
            // If the type is a nested type, we need to convert it from the form Namespace.ParentType.NestedType to Namespace.ParentType+NestedType.
            // We don't know how many dots from the end need to be replaced with a +, if any. So if we fail with all dots, we keep replacing dots until it works!
            string adjustedTypeName = fullName;
            while (true)
            {
                foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = ass.GetType(adjustedTypeName);

                    if (type != null)
                        return type;
                }

                int lastDotIndex = adjustedTypeName.LastIndexOf('.');
                if (lastDotIndex == -1)
                    break; // No more dots to replace, type resolution failed

                // Replace the last dot with a plus sign and try again
                adjustedTypeName = adjustedTypeName.Substring(0, lastDotIndex) + '+' + adjustedTypeName.Substring(lastDotIndex + 1);
            }

            throw new FormatException($"Cannot parse text {fullName} as System.Type");
        }


        private class ParsingType
        {
            public string TypeFullName;
            public List<ParsingType> GenericParameters = new List<ParsingType>();

            public Type TurnIntoType()
            {
                if (GenericParameters.Count == 0)
                    return ParseTypeFromFullName(TypeFullName);


                var genericTypeBase = ParseTypeFromFullName(TypeFullName + "`" + GenericParameters.Count);

                if (String.IsNullOrEmpty(GenericParameters[0].TypeFullName))
                    return genericTypeBase;

                var genericParameterTypes = GenericParameters.Select(t => t.TurnIntoType()).ToArray();
                return genericTypeBase.MakeGenericType(genericParameterTypes);
            }
        }
    }
}
