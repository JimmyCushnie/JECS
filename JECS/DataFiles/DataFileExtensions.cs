using System;
using System.Collections.Generic;
using JECS.Abstractions;
using JECS.ParsingLogic;
using JECS.ParsingLogic.CollectionTypes;

namespace JECS
{
    public static class DataFileExtensions
    {
        /// <summary> Interpret this file as an object of type T, using that type's fields and properties as top-level keys. </summary>
        public static T GetAsObject<T>(this ReadableDataFile dataFile) => (T)GetAsObjectNonGeneric(dataFile, typeof(T));

        /// <summary> Non-generic version of GetAsObject. You probably want to use GetAsObject. </summary>
        /// <param name="type"> the type to get this object as </param>
        public static object GetAsObjectNonGeneric(this ReadableDataFile dataFile, Type type)
        {
            if (TypeRequiresSubKeyWhenWholeFileIsObject(type))
                return dataFile.GetNonGeneric(type, KEY_SAVED_OBJECT_VALUE);


            if (type.IsNullableType() && dataFile.TopLevelKeys.Count == 0)
                return null;

            if (type.IsAbstract || type.IsInterface)
            {
                var concreteType = dataFile.Get<Type>(ComplexTypes.KEY_CONCRETE_TYPE);
                if (concreteType == null)
                    throw new Exception($"Cannot load file {dataFile.Identifier} as type {type}, because it's an abstract or interface type and the concrete type is not specified.");

                type = concreteType;
            }
            
            object returnThis = Activator.CreateInstance(type);

            foreach (var m in type.GetValidMembers())
            {
                var value = dataFile.GetNonGeneric(m.MemberType, m.Name, m.GetValue(returnThis));
                m.SetValue(returnThis, value);
            }

            return returnThis;
        }


        /// <summary> Save this file as an object of type T, using that type's fields and properties as top-level keys. </summary>
        public static void SaveAsObject<T>(this ReadableWritableDataFile dataFile, T saveThis) => SaveAsObjectNonGeneric(dataFile, typeof(T), saveThis);

        /// <summary> Non-generic version of SaveAsObject. You probably want to use SaveAsObject. </summary>
        /// <param name="type"> what type to save this object as </param>
        /// <param name="saveThis"> the object to save </param>
        public static void SaveAsObjectNonGeneric(this ReadableWritableDataFile dataFile, Type type, object saveThis)
        {
            bool previousAutosaveValue = dataFile.AutoSave;
            dataFile.AutoSave = false; // Don't write to disk when we don't have to

            try
            {
                SetFileContents();
            }
            finally
            {
                dataFile.AutoSave = previousAutosaveValue;
            }

            void SetFileContents()
            {
                if (TypeRequiresSubKeyWhenWholeFileIsObject(type))
                {
                    dataFile.SetNonGeneric(type, KEY_SAVED_OBJECT_VALUE, saveThis);
                    return;
                }


                if (saveThis == null)
                {
                    dataFile.DeleteAllKeys();
                    return;
                }

                if (type.IsAbstract || type.IsInterface)
                {
                    var concreteType = saveThis.GetType();
                    dataFile.Set<Type>(ComplexTypes.KEY_CONCRETE_TYPE, concreteType);

                    type = concreteType;
                }
                
                foreach (var m in type.GetValidMembers())
                    dataFile.SetNonGeneric(m.MemberType, m.Name, m.GetValue(saveThis));
            }
        }

        private static bool TypeRequiresSubKeyWhenWholeFileIsObject(Type type)
        {
            if (BaseTypesManager.IsBaseType(type))
                return true;
            
            var underlyingNullableType = Nullable.GetUnderlyingType(type);
            if (underlyingNullableType != null && BaseTypesManager.IsBaseType(underlyingNullableType))
                return true;

            if (CollectionTypesManager.IsSupportedType(type))
                return true;

            return false;
        }

        private const string KEY_SAVED_OBJECT_VALUE = "value";
        
        
        
        
        
        
        /// <summary> Interpret this file as a dictionary. Top-level keys in the file are interpreted as keys in the dictionary. </summary>
        /// <remarks> <see cref="TKey"/> must be a Base Type </remarks>
        public static Dictionary<TKey, TValue> GetAsDictionary<TKey, TValue>(this ReadableDataFile dataFile)
        {
            if (!BaseTypesManager.IsBaseType(typeof(TKey)))
                throw new Exception($"{nameof(TKey)} must be a base type");

            var keys = dataFile.TopLevelKeys;
            var dictionary = new Dictionary<TKey, TValue>(capacity: keys.Count);

            foreach (string keyText in keys)
            {
                TKey keyObject = BaseTypesManager.ParseBaseType<TKey>(keyText);
                TValue value = dataFile.Get<TValue>(keyText);
                dictionary.Add(keyObject, value);
            }

            return dictionary;
        }

        /// <summary> Save this file as a dictionary, using the dictionary's keys as top-level keys in the file. </summary>
        /// <remarks> <see cref="TKey"/> must be a Base Type </remarks>
        public static void SaveAsDictionary<TKey, TValue>(this ReadableWritableDataFile dataFile, IDictionary<TKey, TValue> dictionary)
        {
            if (!BaseTypesManager.IsBaseType(typeof(TKey)))
                throw new Exception($"{nameof(TKey)} must be a base type");

            bool previousAutosaveValue = dataFile.AutoSave;
            dataFile.AutoSave = false; // Don't write to disk when we don't have to

            try
            {
                SetFileContents();
            }
            finally
            {
                dataFile.AutoSave = previousAutosaveValue;
            }

            void SetFileContents()
            {
                var currentKeys = new List<string>(capacity: dictionary.Count);
                foreach (var key in dictionary.Keys)
                {
                    var keyText = BaseTypesManager.SerializeBaseType(key, dataFile.Style);
                    if (!Utilities.IsValidKey(keyText, out string whyNot))
                        throw new Exception($"Can't save file as this dictionary. A key ({keyText}) is not valid: {whyNot}");

                    currentKeys.Add(keyText);
                    dataFile.Set(keyText, dictionary[key]);
                }

                // Make sure that old data in the file is deleted when a new dictionary is saved.
                foreach (var key in dataFile.TopLevelKeys)
                {
                    if (!currentKeys.Contains(key))
                        dataFile.TopLevelNodes.Remove(key);
                }
            }
        }
    }
}
