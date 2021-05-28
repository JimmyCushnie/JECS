using System;
using System.Linq;

namespace SUCC.BuiltInBaseTypeRules
{
    internal class BaseTypeLogic_Bool : StyledBaseTypeLogic<bool>
    {
        public override string SerializeItem(bool value, FileStyle style)
        {
            switch (style.BoolStyle)
            {
                case BoolStyle.true_false:
                default:
                    return value ? "true" : "false";

                case BoolStyle.on_off:
                    return value ? "on" : "off";

                case BoolStyle.yes_no:
                    return value ? "yes" : "no";

                case BoolStyle.y_n:
                    return value ? "y" : "n";
            }
        }

        private static readonly string[] TrueStrings = new string[] { "true", "on", "yes", "y", };
        private static readonly string[] FalseStrings = new string[] { "false", "off", "no", "n", };
        public override bool ParseItem(string text)
        {
            text = text.ToLower();

            if (TrueStrings.Contains(text)) 
                return true;

            if (FalseStrings.Contains(text)) 
                return false;

            throw new FormatException($"Cannot parse text {text} as boolean");
        }
    }
}
