using System;
using System.Globalization;

namespace SUCC.BuiltInBaseTypeRules
{
    internal class BaseTypeLogic_DateTime : BaseTypeLogic<DateTime>
    {
        const string SuccDateTimeFormat = "yyyy-MM-dd HH:mm:ss";


        public override string SerializeItem(DateTime value)
        {
            return value.ToString(SuccDateTimeFormat);
        }

        public override DateTime ParseItem(string text)
        {
            if (DateTime.TryParseExact(text, SuccDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                return result;

            throw new Exception("Invalid shit!!");
        }
    }
}
