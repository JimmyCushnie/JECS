using System;
using System.Collections.Generic;
using JECS.BuiltInBaseTypeLogics;

namespace JECS.ParsingLogic
{
    /// <summary>
    /// Manages JECS's database of Base Types. https://github.com/JimmyCushnie/JECS/wiki/Base-Types
    /// </summary>
    public static class BaseTypesManager
    {
        static BaseTypesManager()
        {
            RegisterBuiltInBaseTypes();

            void RegisterBuiltInBaseTypes()
            {
                RegisterBaseType(new BaseTypeLogic_Bool());
                RegisterBaseType(new BaseTypeLogic_Char());
                RegisterBaseType(new BaseTypeLogic_DateTime());
                RegisterBaseType(new BaseTypeLogic_IPAddress());
                RegisterBaseType(new BaseTypeLogic_String());
                RegisterBaseType(new BaseTypeLogic_Type());
                RegisterBaseType(new BaseTypeLogic_Version());

                RegisterBaseType(new BaseTypeLogic_DirectoryInfo());
                RegisterBaseType(new BaseTypeLogic_FileInfo());

                RegisterBaseType(new BaseTypeLogic_Float());
                RegisterBaseType(new BaseTypeLogic_Double());
                RegisterBaseType(new BaseTypeLogic_Decimal());

                RegisterBaseType(new BaseTypeLogic_Sbyte());
                RegisterBaseType(new BaseTypeLogic_Byte());
                RegisterBaseType(new BaseTypeLogic_Short());
                RegisterBaseType(new BaseTypeLogic_Ushort());
                RegisterBaseType(new BaseTypeLogic_Int());
                RegisterBaseType(new BaseTypeLogic_Uint());
                RegisterBaseType(new BaseTypeLogic_Long());
                RegisterBaseType(new BaseTypeLogic_Ulong());
                RegisterBaseType(new BaseTypeLogic_BigInt());
            }
        }

        private static readonly Dictionary<Type, BaseTypeLogic> RegisteredBaseTypeLogics = new Dictionary<Type, BaseTypeLogic>();

        public static void RegisterBaseType(BaseTypeLogic logic)
        {
            if (logic == null)
                throw new ArgumentNullException(nameof(logic));

            if (RegisteredBaseTypeLogics.ContainsKey(logic.ApplicableType))
                throw new Exception($"Failed to register base type with logic {logic.GetType()}. There is already a base type logic for type {logic.ApplicableType}. The existing logic is of type {RegisteredBaseTypeLogics[logic.ApplicableType]}");

            RegisteredBaseTypeLogics.Add(logic.ApplicableType, logic);
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
