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

                    lastWrittenLineWasEmpty = line.RawText == string.Empty;

                    if (line is Node node)
                        RecursivelyBuildLines(node.ChildLines);
                }
            }
        }


        private struct ParserData
        {
            public readonly List<Line> topLevelLines;
            public readonly Dictionary<string, KeyNode> topLevelNodes;
            public readonly Stack<Node> nestingNodeStack; // The top of the stack is the node that new nodes should be children of

            private readonly ReadableDataFile dataFile;
            private readonly IEnumerator<(int lineIndex, string line)> iterator;

            private int lastLineIndex;

            public ParserData(ReadableDataFile dataFile, string[] lines)
            {
                topLevelLines = new List<Line>();
                topLevelNodes = new Dictionary<string, KeyNode>();
                nestingNodeStack = new Stack<Node>();
                this.dataFile = dataFile;
                iterator = lines.Select((item, index) => (lineIndex: index, lines: item)).GetEnumerator();
                lastLineIndex = 0; // Unity cannot (yet) do this manually. Wow an int default value.
            }

            public bool TryGetNextLine(out string output)
            {
                output = null;
                if (!iterator.MoveNext())
                    return false;

                var (lineIndex, line) = iterator.Current;
                if (line.Contains('\t'))
                    throw new FormatException("A JECS file cannot contain tabs. Please use spaces instead.");

                lastLineIndex = lineIndex;
                output = line;
                return true;
            }

            public bool TryPeekParentNode(out Node parentNode)
                => nestingNodeStack.TryPeek(out parentNode);

            public Exception ExceptionForLastLine(string message)
                => new InvalidFileStructureException(dataFile, lastLineIndex, message);
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
            // Detect empty files and create a file without any lines.
            // This prevents a final newline from being generated in a file which original was empty.
            if (lines.Length == 1 && lines[0] == "")
                return (new List<Line>(), new Dictionary<string, KeyNode>());

            var parserData = new ParserData(dataFile, lines);

            // Parse the input line by line
            while (parserData.TryGetNextLine(out var line))
            {
                if (LineHasData(line))
                {
                    // The line does have data, thus we can create a data node for it.
                    var newNode = GetNodeFromLine(parserData, line, dataFile);
                    // This new node has to be injected into the node tree.
                    AppropriatelyInjectDataNodeIntoTree(parserData, newNode); // This method is the only place, that will remove nodes from the stack.

                    // Nodes with values cannot have children. Thus, only when there is no value, add it onto the stack.
                    if (!newNode.HasValue)
                        parserData.nestingNodeStack.Push(newNode); // This is the only call, which adds nodes onto the stack.

                    // Except a multi-line string, it can have children. But that is entirely handled in its own method.
                    // Multi-line strings always start with a value """
                    if (newNode.Value == MultiLineStringNode.Terminator)
                        ParseMultiLineString(parserData, dataFile, newNode); // The only other method grabbing new lines.
                }
                else // Line has no data
                {
                    var noDataLine = new Line(line);
                    AppropriatelyInjectNoDataNodeIntoTree(parserData, noDataLine);
                }
            }

            return (parserData.topLevelLines, parserData.topLevelNodes);
        }

        private static void AppropriatelyInjectNoDataNodeIntoTree(ParserData parserData, Line noDataLine)
        {
            if (parserData.TryPeekParentNode(out var parentNode))
                parentNode.AddChild(noDataLine);
            else
                parserData.topLevelLines.Add(noDataLine);
        }

        private static void AppropriatelyInjectDataNodeIntoTree(ParserData parserData, Node nodeToAdd)
        {
            // This method will try to add the node to a parent.
            // For that it will check the stack top-down until it finds a suitable parent.
            // Suitable means, the parent has a lower indentation than the new child node.
            // If there is no such parent (meaning the only stack entry is a sibling), then a new top-level node is added.
            // This method won't add anything to the stack! After adding a top-level node, it leaves a clean table.
            while (true)
            {
                if (!parserData.TryPeekParentNode(out var potentialParentNode))
                {
                    // There is no (potential) parent node, thus we are now adding a top-level node.
                    AddTopLevelNodeToTree(parserData, nodeToAdd);
                    return;
                }

                // We have a potential parent node. Compare indentations to see if it is actually one.
                if (nodeToAdd.IndentationLevel > potentialParentNode.IndentationLevel)
                {
                    // Our node has a HIGHER indentation than the potential parent node, meaning the new node is an actual child of the (potential) parent.
                    AddChildNodeToTree(parserData, potentialParentNode, nodeToAdd);
                    return;
                }

                // Remaining indentation possibilities:
                // - LOWER than potential parent, this means it is a sibling of any parent before that, or it is a top node.
                // - SAME as potential parent, this means it is a sibling of the previous (potential) parent node.
                // In either case, the previously potential parent node does not have any further child nodes, drop it from the stack.
                parserData.nestingNodeStack.Pop();
                // And do the same comparisons again for the next potential parent...
            }
        }

        private static void AddTopLevelNodeToTree(ParserData parserData, Node node)
        {
            if (node is not KeyNode keyNode)
                throw parserData.ExceptionForLastLine("Top level lines must be key nodes");
            if (keyNode.IndentationLevel != 0)
                throw parserData.ExceptionForLastLine("Top level nodes must not have indentation");

            parserData.topLevelLines.Add(node);
            if (!parserData.topLevelNodes.TryAdd(keyNode.Key, keyNode))
                throw parserData.ExceptionForLastLine($"Multiple top level keys called '{keyNode.Key}'");
        }

        private static void AddChildNodeToTree(ParserData parserData, Node parent, Node child)
        {
            if (parent.ChildNodes.Count == 0)
            {
                // A parent node does not control the type of its child nodes. But all children must be of the same type.
                // Thus, the very first child will store its own type in the parent (ChildNodeType), so that future siblings type can be checked.
                parent.ChildNodeType = child switch
                {
                    KeyNode => NodeChildrenType.Key,
                    ListNode => NodeChildrenType.List,
                    _ => throw new Exception($"Impossible to reach code. At this point only Key/List nodes should reach this method. Got '{child.GetType()}' instead")
                };
            }
            else
            {
                // As there already have been previous siblings, make sure that the new sibling has same indentation and type.
                var sibling = parent.ChildNodes[0];

                // Sibling nodes must have exactly the same indentation as its previous sibling, thus by incrementation the first sibling.
                if (child.IndentationLevel != sibling.IndentationLevel)
                    throw parserData.ExceptionForLastLine("Line did not have the same indentation as its assumed sibling");

                // Ensure that the new siblings have the same type.
                // This check also checks for crude errors, where something went horribly wrong.
                if (// Expected checks:
                    parent.ChildNodeType == NodeChildrenType.Key && child is not KeyNode
                    || parent.ChildNodeType == NodeChildrenType.List && child is not ListNode
                    // Fatal failure checks:
                    || parent.ChildNodeType == NodeChildrenType.MultiLineString
                    || parent.ChildNodeType == NodeChildrenType.None)
                    throw parserData.ExceptionForLastLine("Line did not match the child type of its parent");
            }

            // All good, add the child (except not all good, and we get a duplicate key...)
            if(!parent.TryAddChild(child))
                throw parserData.ExceptionForLastLine($"Multiple sibling keys called '{((KeyNode)child).Key}' (indentation level {child.IndentationLevel})");
        }

        private static void ParseMultiLineString(ParserData parserData, ReadableDataFile dataFile, Node parentNode)
        {
            // Set the parent node type to MLS, this can be/is used for validation later on
            parentNode.ChildNodeType = NodeChildrenType.MultiLineString;

            // The target indentation is only known with the very first data/text line.
            int multiLineStringIndentationLevel = -1;

            // Process every line, until another MLS terminator is detected.
            while (parserData.TryGetNextLine(out string line))
            {
                var childNode = new MultiLineStringNode(line, dataFile);

                if (parentNode.ChildNodes.Count == 0)
                {
                    // If this is the first line of the multi-line string, it determines the indentation level.
                    // However, that indentation level must be greater than the parent's.
                    multiLineStringIndentationLevel = childNode.IndentationLevel;

                    if (multiLineStringIndentationLevel <= parentNode.IndentationLevel)
                        throw parserData.ExceptionForLastLine("Multi-line string lines must have an indentation level greater than their parent");
                }
                else
                {
                    if (childNode.IndentationLevel != multiLineStringIndentationLevel)
                        throw parserData.ExceptionForLastLine("Multi-line string lines must all have the same indentation level");
                }

                parentNode.AddChild(childNode);

                // Found the terminator (closer) of this multi-line string. The iteration is over and all lines are processed.
                // Proceed with the main loop outside of this method.
                if (childNode.IsTerminator)
                    return;
            }

            // If the code reaches this point, there was no closing terminator for this MLS.
            throw parserData.ExceptionForLastLine("Multi-line string must be terminated");
        }

        private static bool LineHasData(string line)
        {
            line = line.Trim();
            return line.Length != 0 && line[0] != '#';
        }

        private enum DataLineType { None, Key, List }

        private static Node GetNodeFromLine(ParserData parserData, string line, ReadableDataFile file)
        {
            var dataType = GetDataLineType(line);
            return dataType switch
            {
                DataLineType.Key => new KeyNode(rawText: line, file),
                DataLineType.List => new ListNode(rawText: line, file),
                _ => throw parserData.ExceptionForLastLine("Invalid line type, lines with data must be list or key nodes")
            };
        }

        private static DataLineType GetDataLineType(string line)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0)
                return DataLineType.None;
            if (trimmed[0] == '#')
                return DataLineType.None;
            if (trimmed[0] == '-')
                return DataLineType.List;
            if (trimmed.Contains(':'))
                return DataLineType.Key;

            return DataLineType.None;
        }
    }
}