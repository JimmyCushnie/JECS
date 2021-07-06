using System;

namespace SUCC.BuiltInBaseTypeRules
{
    internal class BaseTypeLogic_Version : BaseTypeLogic<Version>
    {
        public override string SerializeItem(Version value) => value.ToString();

        public override Version ParseItem(string text) => Version.Parse(text);
    }
}
