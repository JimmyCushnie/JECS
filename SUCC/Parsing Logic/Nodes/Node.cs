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

        public List<Line> ChildLines = new List<Line>();
        public List<Node> ChildNodes = new List<Node>();

        public Node(string rawText) : base(rawText) { }
        public Node(int indentation)
        {
            this.IndentationLevel = indentation;
        }


        public KeyNode GetChildAddressedByName(string name)
        {
            foreach (var node in ChildNodes)
            {
                var keynode = node as KeyNode;
                if (keynode.Key == name) return keynode;
            }

            return CreateKeyNode(name);
            KeyNode CreateKeyNode(string key)
            {
                var newnode = new KeyNode(GetProperChildIndentation(), key);

                ChildLines.Add(newnode);
                ChildNodes.Add(newnode);
                return newnode;
            }
        }

        public bool ContainsChildNode(string key)
        {
            foreach (var node in ChildNodes)
            {
                var keynode = node as KeyNode;
                if (keynode.Key == key) return true;
            }
            return false;
        }

        public ListNode GetChildAddressedByListNumber(int number)
        {
            // ensure proper number of child list nodes exist
            var indentation = GetProperChildIndentation();
            for (int i = ChildNodes.Count; i <= number; i++)
            {
                var newnode = new ListNode(indentation);
                ChildNodes.Add(newnode);
                ChildLines.Add(newnode);
            }

            return (ListNode)ChildNodes[number];
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




        public string GetDataText()
        {
            return RawText.Substring(DataStartIndex, DataEndIndex - DataStartIndex)
                .Replace("\\#", "#"); // unescape comments
        }
        public void SetDataText(string newData)
        {
            RawText = 
                RawText.Substring(0, DataStartIndex) 
                + newData 
                + RawText.Substring(DataEndIndex, RawText.Length - DataEndIndex);
            RawText = RawText.Replace("#", "\\#"); // escape comments
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
