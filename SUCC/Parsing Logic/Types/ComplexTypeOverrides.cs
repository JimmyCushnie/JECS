using System;
using System.Collections.Generic;
using System.Reflection;

namespace SUCC.ParsingLogic
{
    /// <summary>
    /// Allows you to set the serialization rules for types in assemblies you don't control.
    /// For types that you can edit, you should use <see cref="DontSaveThisAttribute"/> and <see cref="SaveThisAttribute"/>.
    /// </summary>
    public static class ComplexTypeOverrides
    {
        private static HashSet<PropertyInfo> AlwaysSaveProperies = new HashSet<PropertyInfo>();
        private static HashSet<PropertyInfo> NeverSaveProperies = new HashSet<PropertyInfo>();

        private static HashSet<FieldInfo> AlwaysSaveFields = new HashSet<FieldInfo>();
        private static HashSet<FieldInfo> NeverSaveFields = new HashSet<FieldInfo>();


        /// <summary>
        /// Always save this property, even if it's private.
        /// </summary>
        public static void AlwaysSave(PropertyInfo property)
        {
            if (NeverSaveProperies.Contains(property))
                throw new Exception($"Property ({property}) cannot be both always saved and never saved.");

            AlwaysSaveProperies.Add(property);
        }

        /// <summary>
        /// Never save this property, even if it's public.
        /// </summary>
        public static void NeverSave(PropertyInfo property)
        {
            if (AlwaysSaveProperies.Contains(property))
                throw new Exception($"Property ({property}) cannot be both always saved and never saved.");

            NeverSaveProperies.Add(property);
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

        internal static bool IsAlwaysSaved(PropertyInfo property) => AlwaysSaveProperies.Contains(property);
        internal static bool IsNeverSaved(PropertyInfo property) => NeverSaveProperies.Contains(property);

        internal static bool IsAlwaysSaved(FieldInfo field) => AlwaysSaveFields.Contains(field);
        internal static bool IsNeverSaved(FieldInfo field) => NeverSaveFields.Contains(field);
    }
}
