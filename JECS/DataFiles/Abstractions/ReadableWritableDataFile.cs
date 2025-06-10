using System;
using System.Collections.Generic;
using System.Linq;
using JECS.ParsingLogic;

namespace JECS.Abstractions
{
    /// <summary>
    /// A JECS file that can be both read from and written to.
    /// </summary>
    public abstract class ReadableWritableDataFile : ReadableDataFile
    {
        public ReadableWritableDataFile(string defaultFileText = null) : base(defaultFileText)
        {
        }

        /// <summary> Save the file text to wherever you're storing it </summary>
        protected abstract void SetSavedText(string text);


        /// <summary> Rules for how to format new data saved to this file </summary>
        public FileStyle Style = FileStyle.Default;

        /// <summary> If true, the DataFile will automatically save changes to disk with each Get or Set. If false, you must call SaveAllData() manually. </summary>
        /// <remarks> Be careful with this. You do not want to accidentally be writing to a user's disk at 1000MB/s for 3 hours. </remarks>
        public bool AutoSave
        {
            get => _AutoSave;
            set
            {
                _AutoSave = value;

                if (value == true)
                    SaveAllData();
            }
        }
        private bool _AutoSave = true;

        /// <summary> Serializes the data in this object to the file on disk. </summary>
        public void SaveAllData()
        {
            if (FileDirty)
            {
                string newJECS = GetRawText();
                SetSavedText(newJECS);

                FileDirty = false;
            }
        }

        private bool FileDirty = false;
        protected void MarkFileDirty()
        {
            FileDirty = true;

            if (AutoSave)
                SaveAllData();
        }


        /// <summary> Get some data from the file, saving a new value if the data does not exist </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="defaultValue"> if the key does not exist in the file, this value is saved there and returned </param>
        public override T Get<T>(string key, T defaultValue)
            => base.Get(key, defaultValue);

        /// <summary> Non-generic version of Get. You probably want to use Get. </summary>
        /// <param name="type"> the type to get the data as </param>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="defaultValue"> if the key does not exist in the file, this value is saved there and returned </param>
        public override object GetNonGeneric(Type type, string key, object defaultValue)
        {
            if (TryGetNonGeneric(type, key, out var value))
                return value;

            SetNonGeneric(type, key, defaultValue);
            return defaultValue;
        }

        /// <summary> Save data to the file </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="value"> the value to save </param>
        public void Set<T>(string key, T value)
            => SetNonGeneric(typeof(T), key, value);

        /// <summary> Non-generic version of Set. You probably want to use Set. </summary>
        /// <param name="type"> the type to save the data as </param>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="value"> the value to save </param>
        public void SetNonGeneric(Type type, string key, object value)
        {
            EnsureValueIsCorrectType(type, value);

            if (!TryGetNode(key, out var node))
            {
                // Create a new node and inject it, so that the value can be set in all cases
                node = new KeyNode(indentation: 0, key, file: this);
                TopLevelNodes.Add(key, node);
                TopLevelLines.Add(node);
            }

            NodeManager.SetNodeData(node, value, type, Style);

            MarkFileDirty();
        }


        /// <inheritdoc/>
        public override object GetAtPathNonGeneric(Type type, object defaultValue, params string[] path)
        {
            if (TryGetAtPathNonGeneric(type, out var value, path))
                return value;

            SetAtPathNonGeneric(type, defaultValue, path);
            return defaultValue;
        }

        /// <summary> Like <see cref="Set{T}"/> but works for nested paths instead of just the top level of the file. </summary>
        public void SetAtPath<T>(T value, params string[] path)
            => SetAtPathNonGeneric(typeof(T), value, path);

        /// <summary> Non-generic version of SetAtPath. You probably want to use SetAtPath. </summary>
        public void SetAtPathNonGeneric(Type type, object value, params string[] path)
        {
            EnsureValueIsCorrectType(type, value);

            if (path.Length < 1)
                throw new ArgumentException($"{nameof(path)} must have a length greater than 0");

            if (!TryGetNode(path[0], out var topNode))
            {
                topNode = new KeyNode(indentation: 0, key: path[0], file: this);
                TopLevelNodes.Add(path[0], topNode);
                TopLevelLines.Add(topNode);
            }

            for (int i = 1; i < path.Length; i++)
                topNode = topNode.GetChildAddressedByName(path[i]); // Ensures missing nodes are created

            NodeManager.SetNodeData(topNode, value, type, Style);

            MarkFileDirty();
        }


        /// <summary> Wipes all lines from the file that encode data. </summary>
        public void DeleteAllKeys()
        {
            TopLevelNodes.Clear();
            TopLevelLines.Clear();

            MarkFileDirty();
        }

        /// <summary> Remove a top-level key and all its data from the file </summary>
        public void DeleteKey(string key)
        {
            if (!TryGetNode(key, out var node))
                return;

            TopLevelNodes.Remove(key);
            TopLevelLines.Remove(node);

            MarkFileDirty();
        }

        /// <summary> Like <see cref="DeleteKey"/> but works for nested paths instead of just the top level of the file. </summary>
        public void DeleteKeyAtPath(params string[] path)
        {
            if (path.Length == 1)
            {
                DeleteKey(path[0]);
                return;
            }

            if (path.Length < 1)
                throw new ArgumentException($"{nameof(path)} must have a length greater than 0");

            // At this point path is at least length 2 (due to previous checks)
            string keyOfNodeToDelete = path[^1];
            string[] pathToParent = path[..^1]; // Cut off the last element, due to original length 2+ we still have one element here

            if (!TryGetNodeAtPath(out var parentNode, pathToParent))
                return; // Parent does not exist, so no need to delete a child node anymore

            parentNode.RemoveChild(keyOfNodeToDelete);

            MarkFileDirty();
        }


        /// <summary>
        /// Reset the file to the default data provided when it was created.
        /// </summary>
        public void ResetToDefaultData()
        {
            SetSavedText(DefaultFileCache?.GetRawText() ?? string.Empty);
            ReloadAllData();
        }

        /// <summary>
        /// Reset a value within the file to the default data provided when it was created.
        /// </summary>
        public void ResetValueToDefault(string key)
            => ResetValueAtPathToDefault(key); // The methods are too similar and the overhead of handling paths is negligible.

        /// <summary> Reset a value within the file to the default data provided when it was created. </summary>
        /// <param name="path"> The path pointing to the value in the file. </param>
        public void ResetValueAtPathToDefault(params string[] path)
        {
            if (DefaultFileCache == null || !DefaultFileCache.TryGetNodeAtPath(out var defaultNode, path))
            {
                // There is no default (node), simply delete the key.
                DeleteKeyAtPath(path);
                return;
            }
            // We now have a default value to restore.
            // Path is guaranteed to have at least 1 element, as TryGetNodeAtPath checks that.

            string fileText;
            if (TryGetNode(path[0], out var targetNode))
            {
                // First node exists, as it could be anywhere in the file, find or create the other nodes.
                var pathStack = new Queue<KeyNode>(path.Length);
                pathStack.Enqueue(targetNode);
                // Found the top-level node, continue finding/creating (and remembering) the other nodes.
                // We must create all (missing) nodes on the path, as we have a default value to insert.
                foreach (string pathPart in path[1..])
                {
                    targetNode = targetNode.GetChildAddressedByName(pathPart);
                    pathStack.Enqueue(targetNode);
                }
                // At this point the path exists.

                // Ensure that there are no child lines - these will be fully overwritten (in case that the node already existed).
                targetNode.ClearChildren();

                string rawJecs = GetRawText();
                int index = findIndexInJecs(rawJecs, pathStack);
                string beforeLine = rawJecs[..index]; // Includes a newline at the end.
                // Skip to the end of the line, including a newline. This points to the first character of the next line.
                string afterLine = rawJecs[(index + targetNode.RawText.Length + Utilities.NewLine.Length)..];

                string defaultValueJecs = DataConverter.GetLineTextIncludingChildLines(defaultNode); // This always has a final newline.
                defaultValueJecs = adjustIndentation(defaultValueJecs); // Fix indentation changes, to prevent corruption.
                fileText = beforeLine + defaultValueJecs + afterLine;
            }
            else
            {
                // If the first / top-level node does not exist, one can simply append the default to the file.
                string defaultValueJecs = DataConverter.GetLineTextIncludingChildLines(defaultNode);
                fileText = GetRawText(); // This ends with a newline, we append directly to the end, without blank lines in between.
                fileText += defaultValueJecs;
            }

            SetSavedText(fileText);
            ReloadAllData();


            string adjustIndentation(string value)
            {
                // It is possible, that the user edited a file to have bigger indentation levels.
                // In such cases it is possible, that the default would have less indentation than the key it is inserted in.
                // This results in JECS parsing errors or data corruption.
                // To counter that, it is required that we add or remove indentation level from the default, to make it the same level as the current target node.
                int indentationDelta = targetNode.IndentationLevel - defaultNode.IndentationLevel;
                // Zero means: Keep as-is
                // Positive means: Add spaces to default
                // Negative means: Remove spaces from default (if possible)

                if (indentationDelta > 0)
                {
                    string padding = new string(' ', indentationDelta);
                    string[] lines = value.Split(Utilities.NewLine);

                    // Skip the last line, that is a blank line - must not be padded
                    for (int i = 0; i < lines.Length - 1; i++)
                        lines[i] = padding + lines[i];

                    value = string.Join(Utilities.NewLine, lines);
                }
                else if (indentationDelta < 0)
                {
                    indentationDelta = -indentationDelta; // Invert for ease of use
                    value = string.Join(
                        Utilities.NewLine,
                        value.Split(Utilities.NewLine).Select(line =>
                        {
                            int actualCharacterCountToRemove = 0;
                            // Probe indentationDelta to remove, but never run out of bounds.
                            int max = Math.Min(indentationDelta, line.Length);
                            for (int i = 0; i < max; i++)
                            {
                                if (line[i] != ' ')
                                    break;
                                actualCharacterCountToRemove += 1;
                            }
                            return line[actualCharacterCountToRemove..];
                        })
                    );
                }

                return value;
            }

            // Returns the index of the first character of the target node line.
            int findIndexInJecs(string rawJecsInner, Queue<KeyNode> pathStack)
            {
                // This search algorithm will search the raw JECS value. A string with a trailing newline.
                // We can search for the first occurrence of a node. Any further node can be found by continuing the search from where the last was found.

                // To search we compare the raw-text of a node. The raw text is used to generate the full raw JECS.
                // This way we ensure the same indentation, key and value. And unless the same line exists multiple times, we always will find the correct node.
                // To ensure we are not finding a duplicate, it is mandatory to find the node parents in top-down order first.

                // By finding the parents first, it is ensured, that any previous node/line duplicates are not relevant.
                // Further future duplicates are not found as well, because:
                // - Any nodes with a higher indentation are ignored by match.
                // - To find a duplicate we must first pass a different potential parent with a lower indentation.
                // - Before the indentation lowers, all child nodes (including the one we need) of the last parent appear in the file.
                // Meaning, by finding each parent we will find the position of the correct node. And can replace it from the raw JECS.

                string newline = Utilities.NewLine;
                // To be able to find full lines, the search line is wrapped in newlines. This however implies that the first line starts with a newline.
                rawJecsInner = newline + rawJecsInner;

                // The index will always point onto a newline character. Specifically the one from which we start searching.
                int lastIndex = 0;
                foreach (var pathNode in pathStack)
                {
                    // Again, the found index will point towards the newline starting the current node
                    int currentNodeIndex = rawJecsInner.IndexOf(newline + pathNode.RawText + newline, lastIndex, StringComparison.Ordinal);
                    if (currentNodeIndex < lastIndex) // Better safe than sorry...
                        throw new Exception("Searched for existing nodes raw text, but could not find it in serialized JECS, this should never happen");

                    // We could keep searching from here, or we skip the current node line, by adding its length onto the index
                    lastIndex = currentNodeIndex;
                }
                // Now we got the index of the node we want to edit

                // We do not subtract the newline length, which we added at the beginning.
                // This is for the result to point at the beginning of the line, not at the character in front.
                return lastIndex;
            }
        }
    }
}