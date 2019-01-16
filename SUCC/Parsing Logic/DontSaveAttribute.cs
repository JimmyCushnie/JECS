using System;

namespace SUCC
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DontSaveAttribute : Attribute { }
}
