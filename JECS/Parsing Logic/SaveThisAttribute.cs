using System;

namespace JECS
{
    /// <summary>
    /// Private fields and properties with this attribute WILL be saved and loaded by JECS.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class SaveThisAttribute : Attribute
    {
        /// <summary>
        /// Optional: save this member with a custom name.
        /// </summary>
        public string SaveAs { get; set; }
    }
}
