using JECS.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JECS.ParsingLogic
{
    internal enum NodeChildrenType { None, List, Key, MultiLineString }

    /// <summary>
    /// Represents a line of text in a JECS file that contains data.
    /// </summary>
    internal abstract class Node : Line
    {
        public abstract string Value { get; set; }
        public NodeChildrenType ChildNodeType = NodeChildrenType.None;

        private readonly List<Line> _ChildLines = new List<Line>();
        private readonly List<Node> _ChildNodes = new List<Node>();
        public IReadOnlyList<Line> ChildLines => _ChildLines;
        public IReadOnlyList<Node> ChildNodes => _ChildNodes;


        // This is so the line number of the node can be counted for parsing error messages.
        public readonly ReadableDataFile File;

        // This is here so that nodes can access the style of their file. For nodes part of a ReadOnlyDataFile, it is null.
        // We reference a DataFile rather than a FileStyle so that if a user changes the Style of the File, that change is automatically seen by all its nodes.
        private readonly ReadableWritableDataFile FileStyleRef;


        protected FileStyle Style
            => FileStyleRef?.Style ?? throw new NullReferenceException($"Tried to get the style of a node not part of a {nameof(ReadableWritableDataFile)}");

        /// <summary> This constructor used when loading lines from file </summary>
        public Node(string rawText, ReadableDataFile file) : base(rawText)
        {
            this.File = file ?? throw new ArgumentNullException(nameof(file), "Nodes must belong to a file");
            this.FileStyleRef = file as ReadableWritableDataFile;
        }

        /// <summary> This constructor used when creating new lines to add to the file </summary>
        public Node(int indentation, ReadableDataFile file)
        {
            this.File = file ?? throw new ArgumentNullException(nameof(file), "Nodes must belong to a file");
            this.FileStyleRef = file as ReadableWritableDataFile;

            this.IndentationLevel = indentation;
            this.StyleNotYetApplied = true;
        }

        protected bool StyleNotYetApplied = false;


        public KeyNode GetChildAddressedByName(string name)
        {
            EnsureProperChildType(NodeChildrenType.Key);

            foreach (var node in ChildNodes)
            {
                var keyNode = (KeyNode)node;
                if (keyNode.Key == name) return keyNode;
            }

            return CreateKeyNode(name);
            KeyNode CreateKeyNode(string key)
            {
                var newNode = new KeyNode(GetProperChildIndentation(), key, File);

                AddChild(newNode);
                return newNode;
            }
        }

        public ListNode GetChildAddressedByListNumber(int number)
        {
            EnsureProperChildType(NodeChildrenType.List);

            // ensure proper number of child list nodes exist
            var indentation = GetProperChildIndentation();
            for (int i = ChildNodes.Count; i <= number; i++)
            {
                var newNode = new ListNode(indentation, File);
                AddChild(newNode);
            }

            return ChildNodes[number] as ListNode;
        }

        public MultiLineStringNode GetChildAddressedByStringLineNumber(int number)
        {
            EnsureProperChildType(NodeChildrenType.MultiLineString);

            // ensure proper number of child string nodes exist
            var indentation = GetProperChildIndentation();
            for (int i = ChildNodes.Count; i <= number; i++)
            {
                var newNode = new MultiLineStringNode(indentation, File);
                AddChild(newNode);
            }

            return ChildNodes[number] as MultiLineStringNode;
        }

        private int GetProperChildIndentation()
        {
            // If we already have a child, match new indentation level to that child.
            if (this.ChildNodes.Count > 0)
                return this.ChildNodes[0].IndentationLevel;

            // Otherwise, increase the indentation level in accordance with the FileStyle.
            return this.IndentationLevel + Style.IndentationInterval;
        }

        private void EnsureProperChildType(NodeChildrenType expectedType)
        {
            if (this.HasValue && expectedType != NodeChildrenType.MultiLineString)
                throw new Exception($"node has a value ({Value}), which means it can't have children");

            if (ChildNodeType != expectedType)
            {
                if (ChildNodes.Count == 0)
                    ChildNodeType = expectedType;
                else
                    throw new InvalidOperationException($"can't get child from this node. Expected type was {expectedType}, but node children are of type {ChildNodeType}");
            }
        }


        public bool HasValue => !Value.IsNullOrEmpty();
        public void ClearValue() => Value = string.Empty;

        public bool HasChildNodes => ChildNodes.Count > 0;
        public void ClearChildren()
        {
            _ChildLines.Clear();
            _ChildNodes.Clear();

            ChildNodeType = NodeChildrenType.None;
        }


        public bool ContainsChildNode(string key)
            => GetChildKeys().Contains(key);

        public void AddChild(Line newLine)
        {
            if (this.ChildNodeType == NodeChildrenType.Key && newLine is KeyNode keyNode && this.ContainsChildNode(keyNode.Key))
                throw new ArgumentException($"Tried to add duplicate key node child with key '{keyNode.Key}'");


            if (newLine is Node newNode)
                _ChildNodes.Add(newNode);

            _ChildLines.Add(newLine);
        }

        public void RemoveChild(string key)
        {
            foreach (var node in ChildNodes)
            {
                var keyNode = node as KeyNode;
                if (keyNode?.Key == key)
                {
                    _ChildNodes.Remove(node);
                    _ChildLines.Remove(node);
                    return;
                }
            }
        }

        public void CapChildCount(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Child cap count cannot be negative");

            while (ChildNodes.Count > count)
            {
                var removeThis = ChildNodes.Last();
                _ChildNodes.Remove(removeThis);
                _ChildLines.Remove(removeThis);
            }
        }

        public IEnumerable<string> GetChildKeys()
        {
            if (ChildNodeType != NodeChildrenType.Key)
                yield break;

            foreach (var node in ChildNodes)
                yield return ((KeyNode)node).Key;
        }



        public string GetDataText()
        {
            if (RawText.IsWhitespace())
                return String.Empty;

            return RawText.Substring(DataStartIndex, DataEndIndex - DataStartIndex)
                .Replace("\\#", "#"); // unescape comments
        }
        public void SetDataText(string newData)
        {
            RawText =
                RawText.Substring(0, DataStartIndex) // add preceding whitespace
                + newData.Replace("#", "\\#") // escape comments
                + RawText.Substring(DataEndIndex, RawText.Length - DataEndIndex); // add any following whitespace or comments
        }

        private int DataStartIndex => IndentationLevel;

        private int DataEndIndex
        {
            get
            {
                var text = RawText;

                if (text.IsWhitespace())
                    return text.Length;

                // find the first # in the string
                int PoundSignIndex = text.IndexOf('#');

                while (PoundSignIndex > 0 && text[PoundSignIndex - 1] == '\\') // retry PoundSignIndex if it's escaped by a \
                    PoundSignIndex = text.IndexOf('#', PoundSignIndex + 1);

                // if the string contains a #, remove everything after it
                if (PoundSignIndex > 0)
                    text = text.Substring(0, PoundSignIndex);

                // remove trailing spaces
                text = text.TrimEnd();

                return text.Length;
            }
        }
    }
}
