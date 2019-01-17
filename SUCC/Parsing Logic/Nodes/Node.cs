using System;
using System.Collections.Generic;
using System.Linq;

namespace SUCC
{
    internal enum NodeChildrenType { none, list, key, multiLineString }

    /// <summary>
    /// Represents a line of text in a SUCC file that contains data.
    /// </summary>
    internal abstract class Node : Line
    {
        public abstract string Value { get; set; }
        public NodeChildrenType ChildNodeType = NodeChildrenType.none;

        private List<Line> m_ChildLines = new List<Line>();
        private List<Node> m_ChildNodes = new List<Node>();
        public IReadOnlyList<Line> ChildLines => m_ChildLines;
        public IReadOnlyList<Node> ChildNodes => m_ChildNodes;

        public Node(string rawText) : base(rawText) { }
        public Node(int indentation)
        {
            this.IndentationLevel = indentation;
        }


        public KeyNode GetChildAddressedByName(string name)
        {
            EnsureProperType(NodeChildrenType.key);

            foreach (var node in ChildNodes)
            {
                var keynode = node as KeyNode;
                if (keynode.Key == name) return keynode;
            }

            return CreateKeyNode(name);
            KeyNode CreateKeyNode(string key)
            {
                var newnode = new KeyNode(GetProperChildIndentation(), key);

                AddChild(newnode);
                return newnode;
            }
        }

        public ListNode GetChildAddressedByListNumber(int number)
        {
            EnsureProperType(NodeChildrenType.list);

            // ensure proper number of child list nodes exist
            var indentation = GetProperChildIndentation();
            for (int i = ChildNodes.Count; i <= number; i++)
            {
                var newnode = new ListNode(indentation);
                AddChild(newnode);
            }

            return ChildNodes[number] as ListNode;
        }

        public MultiLineStringNode GetChildAddresedByStringLineNumber(int number)
        {
            EnsureProperType(NodeChildrenType.multiLineString);

            // ensure proper number of child string nodes exist
            var indentation = GetProperChildIndentation();
            for(int i = ChildNodes.Count; i <= number; i++)
            {
                var newnode = new MultiLineStringNode(indentation);
                AddChild(newnode);
            }

            return ChildNodes[number] as MultiLineStringNode;
        }

        private int GetProperChildIndentation()
        {
            int indentation = 0;
            if (this.ChildNodes.Count > 0)
                indentation = this.ChildNodes[0].IndentationLevel; // if we already have a child, match new indentation level to that child
            else
                indentation = this.IndentationLevel + Utilities.IndentationCount; // otherwise, increase the indentation level in accordance with the FileStyle
            return indentation;
        }

        private void EnsureProperType(NodeChildrenType expectedType)
        {
            if (ChildNodeType != expectedType)
            {
                if (ChildNodes.Count == 0)
                    ChildNodeType = expectedType;
                else
                    throw new InvalidOperationException($"can't get child from this node. Expected type was {expectedType}, but node children are of type {ChildNodeType}");
            }
        }



        public bool ContainsChildNode(string key)
            => GetChildKeys().Contains(key);

        public void ClearChildren(NodeChildrenType? newChildrenType = null)
        {
            m_ChildLines.Clear();
            m_ChildNodes.Clear();

            if (newChildrenType != null) ChildNodeType = (NodeChildrenType)newChildrenType;
        }

        public void AddChild(Line newLine)
        {
            m_ChildLines.Add(newLine);

            Node newNode = newLine as Node;
            if (newNode != null) m_ChildNodes.Add(newNode);
        }

        public void RemoveChild(string key)
        {
            foreach (var node in ChildNodes)
            {
                var keynode = node as KeyNode;
                if (keynode?.Key == key)
                {
                    m_ChildNodes.Remove(node);
                    m_ChildLines.Remove(node);
                    return;
                }
            }
        }

        public void CapChildCount(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException("stop it");

            while(ChildNodes.Count > count)
            {
                var removeThis = ChildNodes.Last();
                m_ChildNodes.Remove(removeThis);
                m_ChildLines.Remove(removeThis);
            }
        }

        public string[] GetChildKeys()
        {
            var keys = new string[ChildNodes.Count];

            for(int i = 0; i < ChildNodes.Count; i++)
                keys[i] = (ChildNodes[i] as KeyNode).Key;

            return keys;
        }



        public string GetDataText()
        {
            if (RawText.IsWhitespace()) return String.Empty;

            return RawText.Substring(DataStartIndex, DataEndIndex - DataStartIndex)
                .Replace("\\#", "#"); // unescape comments
        }
        public void SetDataText(string newData)
        {
            RawText = 
                RawText.Substring(0, DataStartIndex) 
                + newData.Replace("#", "\\#") // escape comments
                + RawText.Substring(DataEndIndex, RawText.Length - DataEndIndex);
        }

        private int DataStartIndex => IndentationLevel;

        private int DataEndIndex
        {
            get
            {
                var text = RawText;

                int PoundSignIndex = text.IndexOf('#');

                while (PoundSignIndex > 0 && text[PoundSignIndex - 1] == '\\') // retry PoundSignIndex if it's escaped by a \
                    PoundSignIndex = text.IndexOf('#', PoundSignIndex + 1);

                if (PoundSignIndex > 0)
                    text = text.Substring(0, PoundSignIndex); // remove everything after the comment indicator

                // remove trailing spaces
                text = text.TrimEnd();

                return text.Length;
            }
        }
    }
}
