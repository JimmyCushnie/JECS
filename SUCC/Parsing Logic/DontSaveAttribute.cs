using System;

namespace SUCC
{
    /// <summary>
    /// Fields and properties with this attribute will not be saved and loaded by SUCC.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DontSaveAttribute : Attribute { }
}
