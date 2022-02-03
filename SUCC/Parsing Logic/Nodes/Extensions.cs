using System.Collections.Generic;

namespace SUCC.ParsingLogic
{
    internal static class Extensions
    {
        /// <summary>
        /// Counts lines in a file until it finds the target node.
        /// This is slow, and intended to only be used for parsing errors.
        /// </summary>
        public static int GetLineNumber(this Node targetNode)
        {
            int lineCount = 0;

            recursivelyCount(targetNode.File.TopLevelLines, out int lineNumberOfTarget_);
            return lineNumberOfTarget_;

            void recursivelyCount(IEnumerable<Line> lines, out int lineNumberOfTarget)
            {
                foreach (var line in lines)
                {
                    lineCount++;

                    if (line == targetNode)
                    {
                        lineNumberOfTarget = lineCount;
                        return;
                    }

                    if (line is Node node && node.ChildLines.Count > 0)
                    {
                        recursivelyCount(node.ChildLines, out lineNumberOfTarget);
                        if (lineNumberOfTarget != -1)
                            return;
                    }
                }

                lineNumberOfTarget = -1;
            }
        }
    }
}
