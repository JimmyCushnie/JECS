using System;
using System.Globalization;
using System.Linq;

namespace SUCC.BuiltInBaseTypeRules
{
    internal class BaseTypeRules_Float : BaseTypeLogic<float>
    {
        public override string SerializeItem(float value)
        {
            if (float.IsPositiveInfinity(value))
                return FloatRulesConstants.PositiveInfinityText;

            if (float.IsNegativeInfinity(value))
                return FloatRulesConstants.NegativeInfinityText;

            if (float.IsNaN(value))
                return FloatRulesConstants.NanText;

            return value.ToString(FloatRulesConstants.NoScientificNotationInTostring, NumberFormatInfo.InvariantInfo);
        }

        public override float ParseItem(string text)
        {
            return FloatRulesMathHelpers.ParseFloatWithRationalSupport(text, float.Parse, (float a, float b) => a / b);
        }
    }

    internal class BaseTypeRules_Double : BaseTypeLogic<double>
    {
        public override string SerializeItem(double value)
        {
            if (double.IsPositiveInfinity(value))
                return FloatRulesConstants.PositiveInfinityText;

            if (double.IsNegativeInfinity(value))
                return FloatRulesConstants.NegativeInfinityText;

            if (double.IsNaN(value))
                return FloatRulesConstants.NanText;

            return value.ToString(FloatRulesConstants.NoScientificNotationInTostring, NumberFormatInfo.InvariantInfo);
        }

        public override double ParseItem(string text)
        {
            return FloatRulesMathHelpers.ParseFloatWithRationalSupport(text, double.Parse, (double a, double b) => a / b);
        }
    }

    internal class BaseTypeRules_Decimal : BaseTypeLogic<decimal>
    {
        public override string SerializeItem(decimal value)
        {
            return value.ToString(NumberFormatInfo.InvariantInfo);
        }

        public override decimal ParseItem(string text)
        {
            return FloatRulesMathHelpers.ParseFloatWithRationalSupport(text, decimal.Parse, (decimal a, decimal b) => a / b);
        }
    }


    internal static class FloatRulesConstants
    {
        public const string PositiveInfinityText = "infinity";
        public const string NegativeInfinityText = "-infinity";
        public const string NanText = "nan";

        // This lets us use decimal places instead of scientific notation. Yes, it's horrible.
        // See https://docs.microsoft.com/en-us/dotnet/api/system.single.tostring?view=netframework-4.7.2#System_Single_ToString_System_String_
        public const string NoScientificNotationInTostring = "0.#####################################################################################################################################################################################################################################################################################################################################";
    }

    internal static class FloatRulesMathHelpers
    {
        public static T ParseFloatWithRationalSupport<T>(string text, Func<string, IFormatProvider, T> parseMethod, Func<T, T, T> divideMethod)
        {
            // We only really needed to support one /, but honestly supporting infinity of them is easier.
            if (text.Contains('/'))
            {
                T[] numbers = text.Split('/').Select(parse).ToArray();
                T result = numbers[0];

                for (int i = 1; i < numbers.Length; i++)
                    result = divideMethod.Invoke(result, numbers[i]);

                return result;
            }

            return parse(text);


            T parse(string floatText)
                => parseMethod.Invoke(floatText.ToLower().Trim(), LowercaseParser);
        }

        private static readonly NumberFormatInfo LowercaseParser = new NumberFormatInfo()
        {
            PositiveInfinitySymbol = FloatRulesConstants.PositiveInfinityText,
            NegativeInfinitySymbol = FloatRulesConstants.NegativeInfinityText,
            NaNSymbol = FloatRulesConstants.NanText
        };
    }
}
