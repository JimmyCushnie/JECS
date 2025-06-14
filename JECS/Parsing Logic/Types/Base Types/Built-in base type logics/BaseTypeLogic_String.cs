﻿using JECS.ParsingLogic;

namespace JECS.BuiltInBaseTypeLogics
{
    internal class BaseTypeLogic_String : StyledBaseTypeLogic<string>
    {
        public override string SerializeItem(string value, FileStyle style)
        {
            if (value.IsNullOrEmpty())
                return string.Empty;

            value = value.Replace("\t", "    "); // JECS files cannot contain tabs. Prevent saving strings with tabs in them.

            if (
                style.AlwaysQuoteStrings
                || value[0] == ' '
                || value[value.Length - 1] == ' '
                || value.IsQuoted()
                || value == Utilities.NullIndicator
                )
                value = value.Quote();

            return value;
        }

        public override string ParseItem(string text)
        {
            if (text.IsQuoted())
                return text.UnQuote();

            return text;
        }
    }
}
