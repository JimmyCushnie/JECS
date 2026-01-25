using System;

namespace JECS
{
    /// <summary>
    /// Private fields and properties with this attribute WILL be loaded by JECS but WONT be written when saving.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class OnlyReadThisAttribute : SaveThisAttribute
    {
    }
}
