using System;
using System.Collections.Generic;
using System.Reflection;

namespace SUCC.ParsingLogic
{
    /// <summary>
    /// Manages SUCC's database of Base Types. https://github.com/JimmyCushnie/SUCC/wiki/Base-Types
    /// </summary>
    public static class BaseTypesManager
    {
        private static readonly Dictionary<Type, BaseTypeLogic> RegisteredBaseTypeLogics = new Dictionary<Type, BaseTypeLogic>();

        static BaseTypesManager()
        {
            ReloadBaseTypes();
        }

        /// <summary>
        /// Re-scans the loaded assemblies for base type logic implementations.
        /// </summary>
        public static void ReloadBaseTypes()
        {
            RegisteredBaseTypeLogics.Clear();

            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Not sure why, but these particular assemblies cause weird errors. I get them when running SUCC in Unity.
                // Microsoft.CodeAnalysis is unlikely to ever add custom SUCC base type rules so I feel safe skipping them
                if (ass.FullName.StartsWith("Microsoft.CodeAnalysis"))
                    continue;

                LoadBaseTypeLogicsIn(ass);
            }
        }

        public static void LoadBaseTypeLogicsIn(Assembly ass)
        {
            foreach (var type in ass.GetTypes())
            {
                if (typeof(BaseTypeLogic).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericType && !type.IsGenericTypeDefinition)
                {
                    var logic = Activator.CreateInstance(type) as BaseTypeLogic;

                    if (RegisteredBaseTypeLogics.ContainsKey(logic.ApplicableType))
                        throw new Exception($"Can't load base type logics, duplicate logics for type {logic.ApplicableType}");

                    RegisteredBaseTypeLogics.Add(logic.ApplicableType, logic);
                }
            }
        }

        /// <summary> Returns true if the type is a registered base type. </summary>
        public static bool IsBaseType(Type type)
        {
            if (type.IsEnum)
                return true;

            if (RegisteredBaseTypeLogics.ContainsKey(type))
                return true;

            return false;
        }


        public static string SerializeBaseType<T>(T data, FileStyle style) => SerializeBaseType(data, typeof(T), style);
        public static string SerializeBaseType(object data, Type type, FileStyle style)
        {
            if (RegisteredBaseTypeLogics.TryGetValue(type, out var baseTypeLogic))
                return baseTypeLogic.SerializeObject(data, style);

            if (type.IsEnum)
                return SerializeEnum(data, style);

            throw new Exception($"Cannot serialize base type {type} - are you sure it is a base type?");
        }


        public static T ParseBaseType<T>(string text) => (T)ParseBaseType(text, typeof(T));
        public static object ParseBaseType(string text, Type type)
        {
            if (TryParseBaseType(text, type, out var result))
                return result;

            throw new Exception($"Failed to parse text '{text}' as base type {type}");
        }

        public static bool TryParseBaseType<T>(string text, out T result)
        {
            bool success = TryParseBaseType(text, typeof(T), out object _result);
            result = (T)_result;
            return success;
        }
        public static bool TryParseBaseType(string text, Type type, out object result)
        {
            if (!IsBaseType(type))
                throw new Exception($"Cannot parse: {type} is not a base type");


            try
            {
                if (type.IsEnum)
                {
                    result = ParseEnum(text, type);
                    return true;
                }

                result = RegisteredBaseTypeLogics[type].ParseObject(text);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }




        private static string SerializeEnum(object value, FileStyle style)
        {
            switch (style.EnumStyle)
            {
                case EnumStyle.name: default:
                    return value.ToString();
                case EnumStyle.number:
                    return ((Enum)value).ToString("d");
            }
        }
        private static object ParseEnum(string text, Type type)
        {
            return Enum.Parse(type, text, ignoreCase: true);
        }
    }
}
