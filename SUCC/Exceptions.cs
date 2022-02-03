using SUCC.Abstractions;
using SUCC.ParsingLogic;
using System;

namespace SUCC
{
    /// <summary>
    /// Base class for exceptions that are thrown when there are issues with parsing SUCC text data.
    /// </summary>
    public abstract class ExceptionalSUCC : System.Exception
    {
        internal ExceptionalSUCC() { }
        internal ExceptionalSUCC(string message) : base(message) { }
    }

    /// <summary>
    /// An exception thrown when SUCC cannot parse a text file into a valid tree of SUCC data.
    /// </summary>
    public class InvalidFileStructureException : ExceptionalSUCC
    {
        ReadableDataFile DataFile { get; }
        int LineIndex { get; }

        internal InvalidFileStructureException(ReadableDataFile dataFile, int lineIndex, string message) : base(message)
        {
            DataFile = dataFile;
            LineIndex = lineIndex;
        }

        public override string ToString()
        {
            return $"Invalid file structure on line {LineIndex + 1} of {DataFile}: {Message}";
        }
    }

    /// <summary>
    /// An exception thrown when a node in a SUCC data tree cannot be parsed as the requested type.
    /// </summary>
    public class CannotRetrieveDataFromNodeException : ExceptionalSUCC
    {
        Node ErroneousNode { get; }
        Type ExpectedDataType { get; }

        internal CannotRetrieveDataFromNodeException(Node erroneousNode, Type expectedDataType)
        {
            ErroneousNode = erroneousNode;
            ExpectedDataType = expectedDataType;
        }

        public override string ToString()
        {
            int lineNumber = ErroneousNode.GetLineNumber();
            var file = ErroneousNode.File;

            string startOfErrorMessage = $"Invalid file data on line {lineNumber} of {file}. Expected data of type {ExpectedDataType}, but couldn't interpret data as that type";

            bool nodeHasChildren = ErroneousNode.ChildNodes.Count == 0;
            bool nodeHasValue = !ErroneousNode.Value.IsNullOrEmpty();

            if (nodeHasChildren)
                return startOfErrorMessage + $" (node has {ErroneousNode.ChildNodes.Count} children)";

            if (nodeHasValue)
                return startOfErrorMessage + $": {ErroneousNode.Value}";

            return startOfErrorMessage + ".";
        }
    }
}