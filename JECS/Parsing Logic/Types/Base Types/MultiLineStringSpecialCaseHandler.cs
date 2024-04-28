using JECS.BuiltInBaseTypeRules;
using System;

namespace JECS.ParsingLogic
{
    internal static class MultiLineStringSpecialCaseHandler
    {
        private static readonly BaseTypeLogic_String BaseStringRules = new BaseTypeLogic_String();


        internal static void SetStringSpecialCase(Node node, string value, FileStyle style)
        {
            if (value != null && value.ContainsNewLine())
            {
                node.Value = MultiLineStringNode.Terminator;
                var lines = value.SplitIntoLines();

                node.CapChildCount(lines.Length + 1);

                for (int i = 0; i < lines.Length; i++)
                {
                    var newNode = node.GetChildAddressedByStringLineNumber(i);
                    string lineValue = BaseStringRules.SerializeItem(lines[i], style);

                    if (lineValue.EndsWith(MultiLineStringNode.NoLineBreakIndicator))
                        lineValue = lineValue.Quote();

                    newNode.Value = lineValue;
                }

                node.GetChildAddressedByStringLineNumber(lines.Length).MakeTerminator();
                return;
            }
            else
            {
                node.ClearChildren();
                node.Value = BaseStringRules.SerializeItem(value, style);
            }
        }

        internal static string ParseSpecialStringCase(Node parentNode)
        {
            string text = string.Empty;

            for (int i = 0; i < parentNode.ChildNodes.Count; i++)
            {
                var lineNode = parentNode.ChildNodes[i] as MultiLineStringNode;

                if (i == parentNode.ChildNodes.Count - 1)
                {
                    if (lineNode.IsTerminator)
                        break;
                    else
                        throw new FormatException($"Error parsing multi line string: the final child was not a terminator. Line so far was '{text}'");
                }

                bool escapeLineBreak = lineNode.Value.EndsWith(MultiLineStringNode.NoLineBreakIndicator);
                bool isLineBeforeTerminator = i == parentNode.ChildNodes.Count - 2;

                if (escapeLineBreak)
                {
                    if (isLineBeforeTerminator)
                        text += lineNode.Value;
                    else
                        text += lineNode.Value.Remove(lineNode.Value.Length - MultiLineStringNode.NoLineBreakIndicator.Length);
                }
                else
                {
                    text += (string)BaseStringRules.ParseItem(lineNode.Value);

                    if (!isLineBeforeTerminator)
                        text += Utilities.NewLine;
                }
            }

            return text;
        }
    }
}
