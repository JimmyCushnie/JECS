using System;

namespace SUCC.InternalParsingLogic
{
    /// <summary>
    /// Represents a single line of text in a SUCC file.
    /// </summary>
    internal class Line
    {
        public Line() { }
        public Line(string rawText)
        {
            this.RawText = rawText;
        }

        public string RawText { get; set; }
        public int IndentationLevel
        {
            get => RawText.GetIndentationLevel();
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException($"node indents must be at least 0. You tried to set it to {value}");

                var indent = RawText.GetIndentationLevel();
                if (value == indent) return;

                var diff = value - indent;
                if (diff > 0)
                    RawText = new string(' ', diff) + RawText;
                else
                    RawText = RawText.Substring(startIndex: -diff);
            }
        }
    }
}
