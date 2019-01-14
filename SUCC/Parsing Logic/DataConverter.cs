using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using static SUCC.Utilities;

namespace SUCC
{
    internal static class DataConverter
    {
        /// <summary>
        /// Parses a string of SUCC into a data structure
        /// </summary>
        internal static (List<Line>, Dictionary<string, KeyNode>) DataStructureFromSUCC(string input)
        {
            var lines = new List<string>();
            // parse the input line by line
            using (StringReader sr = new StringReader(input))
            {
                string line;
                while ((line = sr.ReadLine()) != null) // this is effectively a ForEachLine, but it is platform agnostic (since new lines are encoded differently on different OSs)
                {
                    lines.Add(line);
                }
            }

            return DataStructureFromSUCC(lines.ToArray());
        }

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
            foreach(var line in lines)
            {
                if (line.Contains('\t')) throw new FormatException("a SUCC file cannot contain tabs. Please use spaces instead.");

                if (DoingMultiLineString)
                {
                    if (NestingNodeStack.Peek().ChildNodeType != NodeChildrenType.multiLineString)
                        throw new Exception("oh fuck, we were supposed to be doing a multi-line string but the top of the node stack isn't a multi-line string node!");

                    NestingNodeStack.Peek().ChildLines.Add(new Line() { RawText = line });

                    var text = line;

                    // remove everything after the comment indicator
                    int PoundSignIndex = text.IndexOf('#');
                    if (PoundSignIndex > 0)
                        text = text.Substring(0, PoundSignIndex - 1);

                    if (text.Trim() == "\"\"\"")
                    {
                        DoingMultiLineString = false;
                        NestingNodeStack.Pop();
                    }

                    continue;
                }

                if (LineHasData(line))
                {
                    var DataType = GetDataLineType(line);
                    Node node = null;
                    switch (DataType)
                    {
                        case DataLineType.key:
                            node = new KeyNode() { RawText = line };
                            break;
                        case DataLineType.list:
                            node = new ListNode() { RawText = line };
                            break;

                        default:
                            throw new FormatException("a data line was not in a valid format. Try commenting it out. Line text was '" + line + "'");
                    }

                    boobies:

                    if (NestingNodeStack.Count == 0)
                    {
                        if (!(node is KeyNode))
                            throw new FormatException("top level lines must be key nodes. The following line does not conform to this: '" + line + "'");
                        TopLevelLines.Add(node);
                        KeyNode heck = (KeyNode)node;
                        TopLevelNodes.Add(heck.Key, heck);
                    }
                    else
                    {
                        int StackTopIndentation = LineIndentationLevel(NestingNodeStack.Peek().RawText);
                        int LineIndentation = LineIndentationLevel(line);

                        if (LineIndentation > StackTopIndentation)
                        {
                            if (NestingNodeStack.Peek().ChildNodes.Count > 0)
                            {
                                int SiblingIndentation = LineIndentationLevel(NestingNodeStack.Peek().ChildNodes[0].RawText);
                                if (LineIndentation != SiblingIndentation)
                                    throw new FormatException("The following line: '" + line + "' did not have the same indentation as its assumed sibling, '" + NestingNodeStack.Peek().ChildLines.First().RawText + "'");

                                if ((NestingNodeStack.Peek().ChildNodeType == NodeChildrenType.key && !(node is KeyNode))
                                    || (NestingNodeStack.Peek().ChildNodeType == NodeChildrenType.list && !(node is ListNode))
                                    || NestingNodeStack.Peek().ChildNodeType == NodeChildrenType.multiLineString
                                    || NestingNodeStack.Peek().ChildNodeType == NodeChildrenType.none)
                                    throw new FormatException("The following line: '" + line + "' did not match the child type of its parent, '" + NestingNodeStack.Peek().RawText + "'");
                            }
                            else
                            {
                                if (node is KeyNode)
                                    NestingNodeStack.Peek().ChildNodeType = NodeChildrenType.key;
                                else if (node is ListNode)
                                    NestingNodeStack.Peek().ChildNodeType = NodeChildrenType.list;
                                else
                                    throw new Exception("what the fuck?");
                            }

                            NestingNodeStack.Peek().ChildLines.Add(node);
                            NestingNodeStack.Peek().ChildNodes.Add(node);
                        }
                        else
                        {
                            NestingNodeStack.Pop();
                            goto boobies;
                        }
                    }

                    if (node.Value == "")
                        NestingNodeStack.Push(node);

                    if (node.Value == "\"\"\"")
                    {
                        NestingNodeStack.Push(node);
                        node.ChildNodeType = NodeChildrenType.multiLineString;
                        DoingMultiLineString = true;
                    }
                }
                else // line has no data
                {
                    Line NoDataLine = new Line() { RawText = line };
                    if(NestingNodeStack.Count == 0)
                    {
                        TopLevelLines.Add(NoDataLine);
                    }
                    else
                    {
                        NestingNodeStack.Peek().ChildLines.Add(NoDataLine);
                    }
                }
            }

            return (TopLevelLines, TopLevelNodes);
        }

        public static string SUCCFromDataStructure(List<Line> lines)
        {
            return RecursivelySerializeLines(lines).TrimEnd(Environment.NewLine.ToCharArray()); // remove all newlines at the end of the string

            string RecursivelySerializeLines(List<Line> Lines)
            {
                string output = string.Empty;
                for (int i = 0; i < Lines.Count; i++)
                {
                    output += Lines[i].RawText;
                    output += Environment.NewLine;

                    if (Lines[i] is Node)
                    {
                        Node heck = (Node)Lines[i];
                        output += RecursivelySerializeLines(heck.ChildLines);
                    }
                }

                return output;
            }
        }


        private static bool LineHasData(string line)
        {
            line = line.Trim();
            return line.Length != 0 && line[0] != '#';
        }

        private enum DataLineType { none, key, list }
        private static DataLineType GetDataLineType(string line)
        {
            // remove everything after the comment indicator
            int PoundSignIndex = line.IndexOf('#');
            if (PoundSignIndex > 0)
                line = line.Substring(0, PoundSignIndex - 1);

            line = line.Trim();
            if (line.Length == 0) return DataLineType.none;
            if (line[0] == '#') return DataLineType.none;
            if (line[0] == '-') return DataLineType.list;
            if (line.Contains(':')) return DataLineType.key;

            return DataLineType.none;
        }
    }
}