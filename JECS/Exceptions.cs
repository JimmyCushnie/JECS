using JECS.Abstractions;
using JECS.ParsingLogic;
using System;

namespace JECS
{
    /// <summary>
    /// Base class for exceptions that are thrown when there are issues with parsing JECS text data.
    /// </summary>
    public abstract class JecsParsingException : Exception
    {
    }

    /// <summary>
    /// An exception thrown when JECS cannot parse a text file into a valid tree of JECS data.
    /// </summary>
    public class InvalidFileStructureException : JecsParsingException
    {
        ReadableDataFile DataFile { get; }
        int LineIndex { get; } // Note that this is the line INDEX -- to get the line NUMBER we need to add one (fucking off by one errors)
        string Details { get; }

        internal InvalidFileStructureException(ReadableDataFile dataFile, int lineIndex, string details)
        {
            DataFile = dataFile;
            LineIndex = lineIndex;
            Details = details;
        }

        public override string Message => $"Invalid file structure on line {LineIndex + 1} of {DataFile}: {Details}";
    }

    /// <summary>
    /// An exception thrown when a node in a JECS data tree cannot be parsed as the requested type.
    /// </summary>
    public class CannotRetrieveDataFromNodeException : JecsParsingException
    {
        Node ErroneousNode { get; }
        Type ExpectedDataType { get; }

        internal CannotRetrieveDataFromNodeException(Node erroneousNode, Type expectedDataType)
        {
            ErroneousNode = erroneousNode;
            ExpectedDataType = expectedDataType;
        }

        public override string Message
        {
            get
            {
                int lineNumber = ErroneousNode.GetLineNumber();
                var file = ErroneousNode.File;

                string startOfErrorMessage = $"Invalid file data on line {lineNumber} of {file}. Expected data of type {ExpectedDataType}, but couldn't interpret data as that type";

                if (ErroneousNode.HasChildNodes)
                    return startOfErrorMessage + $" (node has {ErroneousNode.ChildNodes.Count} children)";

                if (ErroneousNode.HasValue)
                    return startOfErrorMessage + $": {ErroneousNode.Value}";

                return startOfErrorMessage + ".";
            }
        }
    }
}