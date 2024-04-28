using JECS.Abstractions;

namespace JECS.ParsingLogic
{
    /// <summary>
    /// Represents a line of text in a JECS file that contains part of a multi-line string.
    /// </summary>
    internal class MultiLineStringNode : Node
    {
        public MultiLineStringNode(string rawText, ReadableDataFile file) : base(rawText, file) { }
        public MultiLineStringNode(int indentation, ReadableDataFile file) : base(indentation, file)
        {
            this.StyleNotYetApplied = false; // currently, no styles apply to MultiLineStringNodes
        }

        public override string Value
        {
            get => GetDataText();
            set => SetDataText(value);
        }


        public static readonly string Terminator = "\"\"\"";

        public bool IsTerminator => Value == Terminator;
        public void MakeTerminator() => Value = Terminator;


        public static readonly string NoLineBreakIndicator = @"\";
    }
}
