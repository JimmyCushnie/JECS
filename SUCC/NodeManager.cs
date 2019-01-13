using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;
using SUCC.Types;

namespace SUCC
{
    public class DontSaveAttribute : Attribute { }

    internal static class NodeManager
    {
        internal static void SetNodeData(Node node, object data, Type type)
        {
            if (data == null)
                throw new Exception("you can't serialize null");

            if (type == typeof(string))
            {
                string dataAsString = (string)data;
                if (dataAsString != null && dataAsString.Contains(Environment.NewLine))
                {
                    node.Value = "\"\"\"";

                    node.ChildLines.Clear();
                    node.ChildNodes.Clear();

                    int indentation = node.IndentationLevel + Utilities.IndentationCount;
                    using (StringReader sr = new StringReader(dataAsString))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null) // this is effectively a ForEachLine, but it is platform agnostic (since new lines are encoded differently on different OSs)
                        {
                            string text = new string(' ', indentation) + SerializeString(line);
                            text = text.Replace("#", "\\#"); // good god this code is a fucking mess
                            Line newline = new Line() { RawText = text };
                            node.ChildLines.Add(newline);
                        }
                    }

                    string endtext = new string(' ', indentation) + "\"\"\"";
                    Line endline = new Line() { RawText = endtext };
                    node.ChildLines.Add(endline);
                    return;
                }
                else
                {
                    node.ChildLines.Clear();
                    node.ChildNodes.Clear();
                }
            }

            else if (BaseTypes.IsBaseType(type))
                node.Value = BaseTypes.SerializeBaseType(data, type);

            else if (CollectionTypes.TrySetCollection(node, data, type))
                return;

            ComplexTypes.SetComplexNode(node, data, type);
        }


        internal static object GetNodeData(Node node, Type type)
        {
            if (type == typeof(string) && node.Value == "\"\"\"" && node.ChildLines.Count > 0)
            {
                string text = string.Empty;

                foreach(var line in node.ChildLines)
                {
                    var lineText = line.RawText;

                    // remove everything after the comment indicator, unless it's preceded by a \

                    int PoundSignIndex = lineText.IndexOf('#');

                    while (PoundSignIndex > 0 && text[PoundSignIndex - 1] == '\\')
                        PoundSignIndex = text.IndexOf('#', PoundSignIndex + 1);

                    if (PoundSignIndex > 0)
                        lineText = lineText.Substring(0, PoundSignIndex - 1);

                    lineText = lineText.Trim();

                    if(lineText == "\"\"\"")
                    {
                        break;
                    }

                    lineText = (string)ParseString(lineText); // to remove quotations

                    text += lineText;
                    text += Environment.NewLine;
                }

                return text.TrimEnd(Environment.NewLine.ToCharArray()); // remove all newlines at the end of the string
            }

            if (BaseTypes.IsBaseType(type))
                return BaseTypes.ParseBaseType(node.Value, type);

            var collection = CollectionTypes.TryGetCollection(node, type);
            if (collection != null) return collection;

            if (!String.IsNullOrEmpty(node.Value))
                ComplexTypeShortcuts.GetFromShortcut(node.Value, type);

            return ComplexTypes.RetrieveComplexType(node, type);
        }
    }
}