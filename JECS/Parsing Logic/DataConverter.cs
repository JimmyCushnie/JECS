using JECS.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JECS.ParsingLogic
{
    internal static class DataConverter
    {
        internal static string GetLineTextIncludingChildLines(Line line)
            => JecsFromDataStructure(new Line[] { line });

        /// <summary>
        /// Turns a data structure into raw JECS.
        /// </summary>
        internal static string JecsFromDataStructure(IReadOnlyList<Line> lines)
        {
            var builder = new StringBuilder();
            bool lastWrittenLineWasEmpty = false;
            RecursivelyBuildLines(lines);

            // All written files should end with a newline. This is convention when generating plaintext files for a number of good reasons.
            // If the last line that already exists is empty, then we already have a newline at the end of the file.
            // In this case, we need to remove the extra newline that was added in RecursivelyBuildLines.
            // Without this check, files gain an extra newline every time they're saved, ad infinitum!
            if (lastWrittenLineWasEmpty)
                builder.Remove(builder.Length - Utilities.NewLine.Length, Utilities.NewLine.Length);

            return builder.ToString();


            void RecursivelyBuildLines(IEnumerable<Line> _lines)
            {
                foreach (var line in _lines)
                {
                    builder.Append(line.RawText);
                    builder.Append(Utilities.NewLine);

                    if (line.RawText == string.Empty)
                        lastWrittenLineWasEmpty = true;

                    if (line is Node node)
                        RecursivelyBuildLines(node.ChildLines);
                }
            }
        }

        /// <summary>
        /// Parses a string of JECS into a data structure.
        /// </summary>
        internal static (List<Line> topLevelLines, Dictionary<string, KeyNode> topLevelNodes) DataStructureFromJecs(string input, ReadableDataFile fileRef)
            => DataStructureFromJecs(input.SplitIntoLines(), fileRef);

        /// <summary>
        /// Parses lines of JECS into a data structure.
        /// </summary>
        internal static (List<Line>, Dictionary<string, KeyNode>) DataStructureFromJecs(string[] lines, ReadableDataFile dataFile) // I am so, so sorry. If you need to understand this function for whatever reason... may god give you guidance.
        {
            // If the file is empty
            // Do this because otherwise new files are created with a newline at the top
            if (lines.Length == 1 && lines[0] == "")
                return (new List<Line>(), new Dictionary<string, KeyNode>());


            var topLevelLines = new List<Line>();
            var topLevelNodes = new Dictionary<string, KeyNode>();

            var nestingNodeStack = new Stack<Node>(); // The top of the stack is the node that new nodes should be children of

            bool doingMultiLineString = false;
            int multiLineStringIndentationLevel = -1;

            // Parse the input line by line
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];
                if (line.Contains('\t'))
                    throw new FormatException("a JECS file cannot contain tabs. Please use spaces instead.");

                if (doingMultiLineString)
                {
                    var parentNode = nestingNodeStack.Peek();

                    if (parentNode.ChildNodeType != NodeChildrenType.MultiLineString)
                        throw new Exception("oh no, we were supposed to be doing a multi-line string but the top of the node stack isn't a multi-line string node!");

                    var newBoi = new MultiLineStringNode(rawText: line, dataFile);

                    if (parentNode.ChildNodes.Count == 0)
                    {
                        // If this is the first line of the multi-line string, it determines the indentation level.
                        // However, that indentation level must be greater than the parent's.
                        multiLineStringIndentationLevel = newBoi.IndentationLevel;

                        if (multiLineStringIndentationLevel <= parentNode.IndentationLevel)
                            throw new InvalidFileStructureException(dataFile, lineIndex, "multi-line string lines must have an indentation level greater than their parent");
                    }
                    else
                    {
                        if (newBoi.IndentationLevel != multiLineStringIndentationLevel)
                            throw new InvalidFileStructureException(dataFile, lineIndex, "multi-line string lines must all have the same indentation level");
                    }

                    parentNode.AddChild(newBoi);

                    if (newBoi.IsTerminator)
                    {
                        doingMultiLineString = false;
                        multiLineStringIndentationLevel = -1;

                        nestingNodeStack.Pop();
                    }

                    continue;
                }

                if (LineHasData(line))
                {
                    Node node = GetNodeFromLine(line, dataFile, lineIndex);

                    addNodeInAppropriatePlaceInStack:

                    if (nestingNodeStack.Count == 0) // If this is a top-level node
                    {
                        if (!(node is KeyNode))
                            throw new InvalidFileStructureException(dataFile, lineIndex, "top level lines must be key nodes");
                        if (node.IndentationLevel != 0)
                            throw new InvalidFileStructureException(dataFile, lineIndex, "top level nodes must not have indentation");

                        topLevelLines.Add(node);
                        KeyNode heck = node as KeyNode;

                        try
                        {
                            topLevelNodes.Add(heck.Key, heck);
                        }
                        catch (ArgumentException)
                        {
                            throw new InvalidFileStructureException(dataFile, lineIndex, $"multiple top level keys called '{heck.Key}'");
                        }
                    }
                    else // If this is NOT a top-level node
                    {
                        int stackTopIndentation = nestingNodeStack.Peek().IndentationLevel;
                        int lineIndentation = line.GetIndentationLevel();

                        if (lineIndentation > stackTopIndentation) // If this should be a child of the stack top
                        {
                            Node newParent = nestingNodeStack.Peek();
                            if (newParent.ChildNodes.Count == 0) // If this is the first child of the parent, assign the parent's child type
                            {
                                if (node is KeyNode)
                                    newParent.ChildNodeType = NodeChildrenType.Key;
                                else if (node is ListNode)
                                    newParent.ChildNodeType = NodeChildrenType.List;
                                else
                                    throw new Exception("what the heck?");
                            }
                            else // If the parent already has children, check for errors with this line
                            {
                                CheckNewSiblingForErrors(child: node, newParent: newParent, dataFile, lineIndex);
                            }

                            try
                            {
                                newParent.AddChild(node);
                            }
                            catch (ArgumentException)
                            {
                                throw new InvalidFileStructureException(dataFile, lineIndex, $"multiple sibling keys called '{(node as KeyNode).Key}' (indentation level {lineIndentation})");
                            }
                        }
                        else // If this should NOT be a child of the stack top
                        {
                            nestingNodeStack.Pop();
                            goto addNodeInAppropriatePlaceInStack;
                        }
                    }

                    if (!node.HasValue) // If this node can have children, but is not the start of a multi-line string
                        nestingNodeStack.Push(node);

                    if (node.Value == MultiLineStringNode.Terminator) // If this is the start of a multi line string
                    {
                        nestingNodeStack.Push(node);
                        node.ChildNodeType = NodeChildrenType.MultiLineString;

                        doingMultiLineString = true;
                    }
                }
                else // Line has no data
                {
                    Line NoDataLine = new Line(rawText: line);

                    if (nestingNodeStack.Count == 0)
                        topLevelLines.Add(NoDataLine);
                    else
                        nestingNodeStack.Peek().AddChild(NoDataLine);
                }
            }

            return (topLevelLines, topLevelNodes);
        }



        private static bool LineHasData(string line)
        {
            line = line.Trim();
            return line.Length != 0 && line[0] != '#';
        }

        private static Node GetNodeFromLine(string line, ReadableDataFile file, int lineNumber)
        {
            var dataType = GetDataLineType(line);
            switch (dataType)
            {
                case DataLineType.key:
                    return new KeyNode(rawText: line, file);
                    
                case DataLineType.list:
                    return new ListNode(rawText: line, file);

                default:
                    throw new InvalidFileStructureException(file, lineNumber, "format error");
            }
        }

        private static void CheckNewSiblingForErrors(Node child, Node newParent, ReadableDataFile dataFile, int lineNumber)
        {
            Node sibling = newParent.ChildNodes[0];
            if (child.IndentationLevel != sibling.IndentationLevel) // if there is a mismatch between the new node's indentation and its sibling's
                throw new InvalidFileStructureException(dataFile, lineNumber, "Line did not have the same indentation as its assumed sibling");

            if (  // if there is a mismatch between the new node's type and its sibling's
                   newParent.ChildNodeType == NodeChildrenType.Key && !(child is KeyNode)
                || newParent.ChildNodeType == NodeChildrenType.List && !(child is ListNode)
                || newParent.ChildNodeType == NodeChildrenType.MultiLineString
                || newParent.ChildNodeType == NodeChildrenType.None)
                throw new InvalidFileStructureException(dataFile, lineNumber, $"Line did not match the child type of its parent");
        }

        private enum DataLineType { none, key, list }
        private static DataLineType GetDataLineType(string line)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0) return DataLineType.none;
            if (trimmed[0] == '#') return DataLineType.none;
            if (trimmed[0] == '-') return DataLineType.list;
            if (trimmed.Contains(':')) return DataLineType.key;

            return DataLineType.none;
        }
    }
}