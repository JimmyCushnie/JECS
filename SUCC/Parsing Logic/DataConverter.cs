using SUCC.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SUCC.ParsingLogic
{
    internal static class DataConverter
    {
        internal static string GetLineTextIncludingChildLines(Line line)
            => SuccFromDataStructure(new Line[] { line });

        /// <summary>
        /// Turns a data structure into raw SUCC
        /// </summary>
        internal static string SuccFromDataStructure(IEnumerable<Line> lines)
        {
            var succBuilder = new StringBuilder();
            RecursivelyBuildLines(lines, succBuilder);
            return FinishSuccBuilder(succBuilder);


            void RecursivelyBuildLines(IEnumerable<Line> _lines, StringBuilder builder)
            {
                foreach (var line in _lines)
                {
                    builder.Append(line.RawText);
                    builder.Append(Utilities.NewLine);

                    if (line is Node node)
                        RecursivelyBuildLines(node.ChildLines, builder);
                }
            }

            string FinishSuccBuilder(StringBuilder builder)
                => builder.ToString().TrimEnd('\n', '\r', ' ');
        }

        /// <summary>
        /// Parses a string of SUCC into a data structure
        /// </summary>
        internal static (List<Line> topLevelLines, Dictionary<string, KeyNode> topLevelNodes) DataStructureFromSucc(string input, ReadableDataFile fileRef)
            => DataStructureFromSucc(input.SplitIntoLines(), fileRef);

        /// <summary>
        /// Parses lines of SUCC into a data structure
        /// </summary>
        internal static (List<Line>, Dictionary<string, KeyNode>) DataStructureFromSucc(string[] lines, ReadableDataFile fileRef) // I am so, so sorry. If you need to understand this function for whatever reason... may god give you guidance.
        {
            // If the file is empty
            // Do this because otherwise new files are created with a newline at the top
            if (lines.Length == 1 && lines[0] == "")
                return (new List<Line>(), new Dictionary<string, KeyNode>());


            var TopLevelLines = new List<Line>();
            var TopLevelNodes = new Dictionary<string, KeyNode>();

            var NestingNodeStack = new Stack<Node>(); // The top of the stack is the node that new nodes should be children of
            bool DoingMultiLineString = false;

            var file = fileRef as DataFile; // This will be null if fileRef is a ReadOnlyDataFile

            // Parse the input line by line
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Contains('\t'))
                    throw new FormatException("a SUCC file cannot contain tabs. Please use spaces instead.");

                if (DoingMultiLineString)
                {
                    if (NestingNodeStack.Peek().ChildNodeType != NodeChildrenType.multiLineString)
                        throw new Exception("oh fuck, we were supposed to be doing a multi-line string but the top of the node stack isn't a multi-line string node!");

                    var newboi = new MultiLineStringNode(rawText: line, file);

                    NestingNodeStack.Peek().AddChild(newboi);

                    if (newboi.IsTerminator)
                    {
                        DoingMultiLineString = false;
                        NestingNodeStack.Pop();
                    }

                    continue;
                }

                if (LineHasData(line))
                {
                    Node node = GetNodeFromLine(line, file);

                    addNodeInAppriatePlaceInStack:

                    if (NestingNodeStack.Count == 0) // If this is a top-level node
                    {
                        if (!(node is KeyNode))
                            throw new FormatException($"top level lines must be key nodes. Line {i} does not conform to this: '{line}'");
                        TopLevelLines.Add(node);
                        KeyNode heck = node as KeyNode;
                        TopLevelNodes.Add(heck.Key, heck);
                    }
                    else // If this is NOT a top-level node
                    {
                        int StackTopIndentation = NestingNodeStack.Peek().IndentationLevel;
                        int LineIndentation = line.GetIndentationLevel();

                        if (LineIndentation > StackTopIndentation) // If this should be a child of the stack top
                        {
                            Node newParent = NestingNodeStack.Peek();
                            if (newParent.ChildNodes.Count == 0) // If this is the first child of the parent, assign the parent's child type
                            {
                                if (node is KeyNode)
                                    newParent.ChildNodeType = NodeChildrenType.key;
                                else if (node is ListNode)
                                    newParent.ChildNodeType = NodeChildrenType.list;
                                else
                                    throw new Exception("what the fuck?");
                            }
                            else // If the parent already has children, check for errors with this line
                            {
                                CheckNewSiblingForErrors(child: node, newParent: newParent);
                            }

                            newParent.AddChild(node);
                        }
                        else // If this should NOT be a child of the stack top
                        {
                            NestingNodeStack.Pop();
                            goto addNodeInAppriatePlaceInStack;
                        }
                    }

                    if (node.Value == "") // If this is a node with children
                        NestingNodeStack.Push(node);

                    if (node.Value == MultiLineStringNode.Terminator) // if this is the start of a multi line string
                    {
                        NestingNodeStack.Push(node);
                        node.ChildNodeType = NodeChildrenType.multiLineString;
                        DoingMultiLineString = true;
                    }
                }
                else // Line has no data
                {
                    Line NoDataLine = new Line(rawText: line);

                    if (NestingNodeStack.Count == 0)
                        TopLevelLines.Add(NoDataLine);
                    else
                        NestingNodeStack.Peek().AddChild(NoDataLine);
                }
            }

            return (TopLevelLines, TopLevelNodes);
        }



        private static bool LineHasData(string line)
        {
            line = line.Trim();
            return line.Length != 0 && line[0] != '#';
        }

        private static Node GetNodeFromLine(string line, DataFile file)
        {
            var DataType = GetDataLineType(line);
            Node node = null;
            switch (DataType)
            {
                case DataLineType.key:
                    node = new KeyNode(rawText: line, file);
                    break;
                case DataLineType.list:
                    node = new ListNode(rawText: line, file);
                    break;

                default:
                    throw new FormatException($"format error on line: {line}");
            }

            return node;
        }

        private static void CheckNewSiblingForErrors(Node child, Node newParent)
        {
            Node sibling = newParent.ChildNodes[0];
            if (child.IndentationLevel != sibling.IndentationLevel) // if there is a mismatch between the new node's indentation and its sibling's
                throw new FormatException($"Line did not have the same indentation as its assumed sibling. Line was '{child.RawText}'; sibling was '{sibling.RawText}'");

            if (  // if there is a mismatch between the new node's type and its sibling's
                   newParent.ChildNodeType == NodeChildrenType.key && !(child is KeyNode)
                || newParent.ChildNodeType == NodeChildrenType.list && !(child is ListNode)
                || newParent.ChildNodeType == NodeChildrenType.multiLineString
                || newParent.ChildNodeType == NodeChildrenType.none)
                throw new FormatException($"Line did not match the child type of its parent. Line was '{child.RawText}'; parent was '{newParent.RawText}'");
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