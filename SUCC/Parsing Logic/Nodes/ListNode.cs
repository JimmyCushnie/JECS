using System;
using System.Collections.Generic;
using System.Linq;

namespace SUCC
{
    /// <summary>
    /// Represents a line of text in a SUCC file that contains data in a list.
    /// </summary>
    internal class ListNode : Node
    {
        public ListNode(string rawText) : base(rawText) { }
        public ListNode(int indentation, FileStyle style) : base(indentation, style)
        {
            RawText += "-";
        }

        public override string Value
        {
            get
            {
                var text = GetDataText();
                int DashIndex = GetDashIndex(text);

                text = text.Substring(startIndex: DashIndex + 1);
                text = text.TrimStart();
                return text;
                // note that trailing spaces are already trimmed in GetDataText()
            }
            set
            {
                // todo add filestyle stuff here
                var text = GetDataText();
                int dashIndex = GetDashIndex(text);

                string afterDash = text.Substring(dashIndex + 1);
                int spacesAfterDash = afterDash.GetIndentationLevel();

                SetDataText(
                    text.Substring(startIndex: 0, length: dashIndex + spacesAfterDash + 1) + value
                    );
            }
        }

        private int GetDashIndex(string text)
        {
            int DashIndex = text.IndexOf('-');

            if (DashIndex < 0)
                throw new FormatException("List node comprised of the following text: " + RawText + " did not contain the character ':'");
            return DashIndex;
        }
    }
}
