using System;
using System.Collections.Generic;
using System.Linq;

namespace SUCC
{
    /// <summary>
    /// Represents a line of text in a SUCC file that contains part of a multi-line string.
    /// </summary>
    internal class MultiLineStringNode : Node
    {
        public MultiLineStringNode(string rawText) : base(rawText) { }
        public MultiLineStringNode(int indentation, FileStyle style) : base(indentation, style)
        {
            this.UnappliedStyle = false; // currently, no styles apply to MultiLineStringNodes
        }

        public override string Value
        {
            get => GetDataText();
            set
            {
                if (RawText.IsWhitespace())
                    RawText += value;
                else
                    SetDataText(value);
            }
        }

        public static readonly string Terminator = "\"\"\"";

        public bool IsTerminator => Value == Terminator;
        public void MakeTerminator() => Value = Terminator;


        //private void NO()
        //    => throw new InvalidOperationException("You can't do that on a multi-line string node!");
    }
}
