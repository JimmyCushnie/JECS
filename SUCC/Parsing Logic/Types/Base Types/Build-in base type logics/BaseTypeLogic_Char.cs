namespace SUCC.BuiltInBaseTypeRules
{
    internal class BaseTypeLogic_Char : BaseTypeLogic<char>
    {
        public override char ParseItem(string text)
        {
            //if (text.Length != 1)
            //    throw new FileFormatException

            return text[0];
        }

        public override string SerializeItem(char value) => value.ToString();
    }
}
