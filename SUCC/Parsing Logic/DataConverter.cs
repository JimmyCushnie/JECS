using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace SUCC
{
    internal static class DataConverter
    {
        /// <summary>
        /// Turns a data structure into raw SUCC
        /// </summary>
        internal static string SUCCFromDataStructure(List<Line> lines)
        {
            return RecursivelySerializeLines(lines).TrimEnd(Utilities.NewLine.ToCharArray()); // remove all newlines at the end of the string

            string RecursivelySerializeLines(IReadOnlyList<Line> Lines)
            {
                string output = string.Empty;
                for (int i = 0; i < Lines.Count; i++)
                {
                    output += Lines[i].RawText;
                    output += Utilities.NewLine;

                    if (Lines[i] is Node)
                    {
                        var heck = Lines[i] as Node;
                        output += RecursivelySerializeLines(heck.ChildLines);
                    }
                }

                return output;
            }
        }

        /// <summary>
        /// Parses a string of SUCC into a data structure
        /// </summary>
        internal static (List<Line>, Dictionary<string, KeyNode>) DataStructureFromSUCC(string input)
            => DataStructureFromSUCC(input.SplitIntoLines());

        /// <summary>
        /// Parses lines of SUCC into a data structure
        /// </summary>
        internal static (List<Line>, Dictionary<string, KeyNode>) DataStructureFromSUCC(string[] lines) // I am so, so sorry. If you need to understand this function for whatever reason... may god give you guidance.
        {
            var TopLevelLines = new List<Line>();
            var TopLevelNodes = new Dictionary<string, KeyNode>();

            var NestingNodeStack = new Stack<Node>();
            bool DoingMultiLineString = false;

            // parse the input line by line
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Contains('\t')) throw new FormatException("a SUCC file cannot contain tabs. Please use spaces instead.");

                if (DoingMultiLineString)
                {
                    if (NestingNodeStack.Peek().ChildNodeType != NodeChildrenType.multiLineString)
                        throw new Exception("oh fuck, we were supposed to be doing a multi-line string but the top of the node stack isn't a multi-line string node!");

                    var newboi = new MultiLineStringNode(rawText: line);

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
                    Node node = GetNodeFromLine(line);

                    boobies:

                    if (NestingNodeStack.Count == 0) // if this is a top-level node
                    {
                        if (!(node is KeyNode))
                            throw new FormatException($"top level lines must be key nodes. Line {i} does not conform to this: '{line}'");
                        TopLevelLines.Add(node);
                        KeyNode heck = node as KeyNode;
                        TopLevelNodes.Add(heck.Key, heck);
                    }
                    else // if this is NOT a top-level node
                    {
                        int StackTopIndentation = NestingNodeStack.Peek().IndentationLevel;
                        int LineIndentation = line.GetIndentationLevel();

                        if (LineIndentation > StackTopIndentation) // if this should be a child of the stack top
                        {
                            Node newParent = NestingNodeStack.Peek();
                            if (newParent.ChildNodes.Count == 0) // if this is the first child of the parent, assign the parent's child type
                            {
                                if (node is KeyNode)
                                    newParent.ChildNodeType = NodeChildrenType.key;
                                else if (node is ListNode)
                                    newParent.ChildNodeType = NodeChildrenType.list;
                                else
                                    throw new Exception("what the fuck?");
                            }
                            else // if the parent already has children, check for errors with this line
                            {
                                CheckNewSiblingForErrors(child: node, newParent: newParent);
                            }

                            newParent.AddChild(node);
                        }
                        else // if this should NOT be a child of the stack top
                        {
                            NestingNodeStack.Pop();
                            goto boobies;
                        }
                    }

                    if (node.Value == "") // if this is a node with children
                        NestingNodeStack.Push(node);

                    if (node.Value == MultiLineStringNode.Terminator) // if this is the start of a multi line string
                    {
                        NestingNodeStack.Push(node);
                        node.ChildNodeType = NodeChildrenType.multiLineString;
                        DoingMultiLineString = true;
                    }
                }
                else // line has no data
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

        private static Node GetNodeFromLine(string line)
        {
            var DataType = GetDataLineType(line);
            Node node = null;
            switch (DataType)
            {
                case DataLineType.key:
                    node = new KeyNode(rawText: line);
                    break;
                case DataLineType.list:
                    node = new ListNode(rawText: line);
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
                   (newParent.ChildNodeType == NodeChildrenType.key && !(child is KeyNode))
                || (newParent.ChildNodeType == NodeChildrenType.list && !(child is ListNode))
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