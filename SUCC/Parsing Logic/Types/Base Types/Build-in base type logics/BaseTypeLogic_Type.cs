using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SUCC.BuiltInBaseTypeRules
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
        // Note to self: if I update this, I should also update the gist.
        // https://gist.github.com/JimmyCushnie/950e09d9f96fab16d7d723c5b1583c3a

        public static string GetCsharpTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                string typeNameWithoutGenerics = type.FullName.Split('`')[0];
                string genericArguments = String.Join(", ", type.GetGenericArguments().Select(GetCsharpTypeName));

                return typeNameWithoutGenerics + "<" + genericArguments + ">";
            }
            else
            {
                return type.FullName;
            }
        }

        public static Type ParseCsharpTypeName(string typeName)
        {
            if (!typeName.Contains('<'))
                return ParseTypeFromAssemblyName(typeName);


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
                throw new Exception("Aw fuck, looks like the type name had some non-matching brackets");

            return typeStack.Pop().TurnIntoType();
        }

        private class ParsingType
        {
            public string TypeFullName;
            public List<ParsingType> GenericParameters = new List<ParsingType>();

            public Type TurnIntoType()
            {
                if (GenericParameters.Count == 0)
                    return ParseTypeFromAssemblyName(TypeFullName);


                var genericTypeBase = ParseTypeFromAssemblyName(TypeFullName + "`" + GenericParameters.Count);

                if (String.IsNullOrEmpty(GenericParameters[0].TypeFullName))
                    return genericTypeBase;

                var genericParameterTypes = GenericParameters.Select(t => t.TurnIntoType()).ToArray();
                return genericTypeBase.MakeGenericType(genericParameterTypes);
            }
        }

        private static Type ParseTypeFromAssemblyName(string assemblyName)
        {
            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = ass.GetType(assemblyName);

                if (type != null)
                    return type;
            }

            throw new FormatException($"Cannot parse text {assemblyName} as System.Type");
        }
    }
}
