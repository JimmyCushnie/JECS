using System;
using System.Collections.Generic;
using System.Reflection;

namespace JECS.ParsingLogic
{
    /// <summary>
    /// Allows you to set the serialization rules for types in assemblies you don't control.
    /// For types that you can edit, you should use <see cref="DontSaveThisAttribute"/> and <see cref="SaveThisAttribute"/>.
    /// </summary>
    public static class ComplexTypeOverrides
    {
        private static HashSet<PropertyInfo> AlwaysSaveProperties = new HashSet<PropertyInfo>();
        private static HashSet<PropertyInfo> NeverSaveProperties = new HashSet<PropertyInfo>();

        private static HashSet<FieldInfo> AlwaysSaveFields = new HashSet<FieldInfo>();
        private static HashSet<FieldInfo> NeverSaveFields = new HashSet<FieldInfo>();


        /// <summary>
        /// Always save this property, even if it's private.
        /// </summary>
        public static void AlwaysSave(PropertyInfo property)
        {
            if (NeverSaveProperties.Contains(property))
                throw new Exception($"Property ({property}) cannot be both always saved and never saved.");

            AlwaysSaveProperties.Add(property);
        }

        /// <summary>
        /// Never save this property, even if it's public.
        /// </summary>
        public static void NeverSave(PropertyInfo property)
        {
            if (AlwaysSaveProperties.Contains(property))
                throw new Exception($"Property ({property}) cannot be both always saved and never saved.");

            NeverSaveProperties.Add(property);
        }


        /// <summary>
        /// Always save this field, even if it's private.
        /// </summary>
        public static void AlwaysSave(FieldInfo field)
        {
            if (NeverSaveFields.Contains(field))
                throw new Exception($"Field ({field}) cannot be both always saved and never saved.");

            AlwaysSaveFields.Add(field);
        }

        /// <summary>
        /// Never save this field, even if it's public.
        /// </summary>
        public static void NeverSave(FieldInfo field)
        {
            if (AlwaysSaveFields.Contains(field))
                throw new Exception($"Field ({field}) cannot be both always saved and never saved.");

            NeverSaveFields.Add(field);
        }

        internal static bool IsAlwaysSaved(PropertyInfo property) => AlwaysSaveProperties.Contains(property);
        internal static bool IsNeverSaved(PropertyInfo property) => NeverSaveProperties.Contains(property);

        internal static bool IsAlwaysSaved(FieldInfo field) => AlwaysSaveFields.Contains(field);
        internal static bool IsNeverSaved(FieldInfo field) => NeverSaveFields.Contains(field);
    }
}
