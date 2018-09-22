using System;
using System.Collections.Generic;

namespace PENIS
{
    public enum NodeChildrenType { none, list, key, multiLineString }

    public class Line
    {
        public string RawText { get; set; }
        public int IndentationLevel { get { return DataConverter.LineIndentationLevel(RawText); } }
    }

    public abstract class Node : Line
    {
        public abstract string Value { get; set; }
        public NodeChildrenType ChildNodeType = NodeChildrenType.none;

        public List<Line> ChildLines = new List<Line>();
        public List<Node> ChildNodes = new List<Node>();

        public KeyNode GetChildAddressedByName(string name)
        {
            foreach (var node in ChildNodes)
            {
                var keynode = node as KeyNode;
                if (keynode.Key == name) { return keynode; }
            }

            int indentation = 0;
            if (ChildNodes.Count > 0) { indentation = ChildNodes[0].IndentationLevel; }
            if (indentation <= IndentationLevel) { indentation = IndentationLevel + Utilities.IndentationCount; }

            var newnode = new KeyNode();
            newnode.RawText = new string(' ', indentation) + name + ':';

            ChildLines.Add(newnode);
            ChildNodes.Add(newnode);
            return newnode;
        }

        public ListNode GetChildAddressedByListNumber(int number)
        {
            int indentation = 0;
            if (ChildNodes.Count > 0) { indentation = ChildNodes[0].IndentationLevel; }
            if (indentation <= IndentationLevel) { indentation = IndentationLevel + Utilities.IndentationCount; }

            for (int i = ChildNodes.Count; i <= number; i++)
            {
                var newnode = new ListNode();
                newnode.RawText = new string(' ', indentation) + '-';
                ChildNodes.Add(newnode);
                ChildLines.Add(newnode);
            }

            return (ListNode)ChildNodes[number];
        }

        public string GetDataText()
        {
            //UnityEngine.Debug.Log(RawText + ", " + DataStartIndex + ", " + DataEndIndex + ", ");
            return RawText.Substring(DataStartIndex, DataEndIndex - DataStartIndex);
        }
        public void SetDataText(string newData)
        {
            RawText = RawText.Substring(0, DataStartIndex) + newData + RawText.Substring(DataEndIndex, RawText.Length - DataEndIndex);
        }

        protected int DataStartIndex
        {
            get
            {
                return IndentationLevel;
            }
        }
        protected int DataEndIndex
        {
            get
            {
                var text = RawText;

                // remove everything after the comment indicator
                int PoundSignIndex = text.IndexOf('#');
                if (PoundSignIndex > 0)
                    text = text.Substring(0, PoundSignIndex);

                // remove trailing spaces
                text = text.TrimEnd();

                return text.Length;
            }
        }
    }

    public class KeyNode : Node
    {
        public string Key
        {
            get
            {
                var text = GetDataText();
                int ColonIndex = text.IndexOf(':');

                if (ColonIndex < 0)
                    throw new FormatException("Key node comprised of the following text: " + RawText + " did not contain the character ':'");

                text = text.Substring(0, ColonIndex);
                text = text.TrimEnd(); // remove trailing spaces
                return text;
            }
        }

        public override string Value
        {
            get
            {
                var text = GetDataText();
                int ColonIndex = text.IndexOf(':');

                if (ColonIndex < 0)
                    throw new FormatException("Key node comprised of the following text: " + RawText + " did not contain the character ':'");

                text = text.Substring(ColonIndex + 1);
                text = text.TrimStart();
                return text;
                // note that trailing spaces are already trimmed in GetDataText()
            }
            set
            {
                var text = GetDataText();
                int ColonIndex = text.IndexOf(':');

                if (ColonIndex < 0)
                    throw new FormatException("Key node comprised of the following text: " + RawText + " did not contain the character ':'");

                string aftercolon = text.Substring(ColonIndex + 1);
                int spaces = DataConverter.LineIndentationLevel(aftercolon);

                if(spaces == 0)
                {
                    text += ' ';
                    spaces = 1;
                }

                string final = text.Substring(0, ColonIndex + spaces + 1) + value;
                SetDataText(final);
            }
        }
    }

    public class ListNode : Node
    {
        public override string Value
        {
            get
            {
                var text = GetDataText();
                int DashIndex = text.IndexOf('-');

                if (DashIndex < 0)
                    throw new FormatException("Key node comprised of the following text: " + RawText + " did not contain the character '-'");

                text = text.Substring(DashIndex + 1);
                text = text.TrimStart();
                return text;
                // note that trailing spaces are already trimmed in GetDataText()
            }
            set
            {
                var text = GetDataText();

                try
                {
                    SetDataText(text.Replace(Value, value));
                }
                catch (Exception)
                {
                    SetDataText("- " + value);
                }
            }
        }
    }
}