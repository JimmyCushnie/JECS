using System.Net;

namespace JECS.BuiltInBaseTypeLogics
{
    internal class BaseTypeLogic_IPAddress : BaseTypeLogic<IPAddress>
    {
        public override string SerializeItem(IPAddress value) => value.ToString();

        public override IPAddress ParseItem(string text) => IPAddress.Parse(text);
    }
}
