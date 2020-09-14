# SUCC Changelog

## v1.2 | some time in the future probably

* you can now save and load null
* the Unity version can now use the main version's `defaultFileText` feature in addition to the Unity-exclusive `defaultFile` feature
* changed some things about the API, it's possible your code will break sorry
* fixed some bugs
* if you supply a `defaultFileText` when creating a `DataFile`, if you use `Get` and you don't provide a default value, the default value will be searched for in the `defaultFileText`
* base types can now be loaded when they are written in the file using complex type rules (#26)
* multi-line strings can now be written in a file with line breaks while ignoring those line breaks when parsed. (#25)
* added `DistributedData` class, and many extension methods for it
* added `DataFile.TryGet`
  * todo: document this, add non-generic version, add path based versions
* added `ReadableWritableDataFile.ResetValueToDefault()`
* added `ReadableWritableDataFile.ResetToDefaultData()`
* added `BaseTypes.TryParseBaseType`
* fixed default data path on Mac OS (Unity version)
* added `ReadableDataFile.Identifier`
  * when creating a `MemoryDataFile`, you can now supply an identifier
* You can now override the serialization rules for types in assemblies you don't control (#15)
* Renamed `[DoSave]` and `[DontSave]` to `[SaveThis]` and `[DontSaveThis]`
* `[SaveThis]` can now be used to save members with a custom name (#27 -- thanks, pipe01!)
* Improved type handling: non-generic methods now work properly when the given value is of a type derived from the given type
* general code cleanup
* added some tests
* Official support for .NET Core 3.1
* fixed exception when creating a new DataFile with default text in a non-existent directory
* fixed improper file names when attempting to create a DataFile with `.` in the path
* Fixed numbers with decimal places using `,` instead of `.` on systems set to European standards
* Saving and loading Type variables is much nicer with generic types

todo update the docs for 1.2

## v1.1 | 2019-11-12

* added [GetAtPath and SetAtPath](https://github.com/JimmyCushnie/SUCC/wiki/Additional-DataFile-Functionality#getatpath-and-setatpath)
* added [MemoryDataFile and MemoryReadOnlyDataFile](https://github.com/JimmyCushnie/SUCC/wiki/Additional-DataFile-Functionality#getatpath-and-setatpath) (in the `SUCC.MemoryFiles` namespace)
* added support for [writing rational floats](https://github.com/JimmyCushnie/SUCC/wiki/Base-Types#floating-point-types) in files; you can now write `1/3` instead of `0.33333`
* [adding custom base types](https://github.com/JimmyCushnie/SUCC/wiki/Adding-Custom-Base-Types) is now much easier & done generically
  * ```csharp
    // old way:
    SUCC.BaseTypes.AddBaseType
        (
        typeof(MyCustomType),
        (object mct) => mct.ToString(),
        (string text) => MyCustomType.Parse(text)
        );
    
    // new way:
    SUCC.BaseTypes.AddBaseType
        (
        mct => mct.ToString(),
        MyCustomType.Parse
        );
    ```
* added [FileName property](https://github.com/JimmyCushnie/SUCC/wiki/Additional-DataFile-Functionality#filepath-and-filename) to `DataFile` and `ReadOnlyDataFile`, as a complement to `FilePath`
* added [SizeOnDisk](https://github.com/JimmyCushnie/SUCC/wiki/Additional-DataFile-Functionality#sizeondisk) property to `DataFile` and `ReadOnlyDataFile`
* added an unordered [TopLevelKeys property](https://github.com/JimmyCushnie/SUCC/wiki/Additional-DataFile-Functionality#toplevelkeys) and renamed GetTopLevelKeys() to GetTopLevelKeysInOrder()
* simplified repository branch structure; there is now only `master` for the standalone version and `unity` for the Unity version
* made `BaseTypes.ParseBaseType` public
* improved performance of saving files
* added XML documentation for more things
* various code cleanup
* tests are much better and use the new MemoryDataFiles
* prevent the user from saving strings with tabs in them (this is invalid SUCC)
* fixed a few issues where invalid files could be generated
* fixed custom base types sometimes not working
* removed WebGL support from the Unity version
* parsing error messages are *slightly* more helpful. There is still a lot of work to be done here though and this will likely be a focus of 1.2.

## v1.0 | 2019-03-29

- refactored a LOT; code is much better
- added [wiki](https://github.com/JimmyCushnie/SUCC/wiki)
- can now be [installed as a package](https://github.com/JimmyCushnie/SUCC/wiki/Installing#as-unity-package) using the Unity Package Manager
- added a version that [doesn't use Unity](https://github.com/JimmyCushnie/SUCC/wiki/Version-Differences)
- added [tests](https://github.com/JimmyCushnie/SUCC/tree/master/SUCC.Tests)
- added [custom base types](https://github.com/JimmyCushnie/SUCC/wiki/Adding-Custom-Base-Types)
- added [ReadOnlyDataFile](https://github.com/JimmyCushnie/SUCC/wiki/Additional-DataFile-Functionality#readonlydatafile)
- added [DataFile.GetAsDictionary and SaveAsDictionary](https://github.com/JimmyCushnie/SUCC/wiki/Additional-DataFile-Functionality#saveget-as-dictionary)
- added [DataFile.AutoReload and DataFile.OnAutoReload](https://github.com/JimmyCushnie/SUCC/wiki/Additional-DataFile-Functionality#autoreload)
- added [Custom Shortcuts](https://github.com/JimmyCushnie/SUCC/wiki/Complex-Type-Shortcuts#custom-shortcuts)
- added [Utilities.SuccFileExists()](https://github.com/JimmyCushnie/SUCC/wiki/Utilities#succfileexists)
- added [DataFile.GetRawText()](https://github.com/JimmyCushnie/SUCC/wiki/Additional-DataFile-Functionality#getrawtext-and-getrawlines)
- added [AutoSave](https://github.com/JimmyCushnie/SUCC/wiki/Additional-DataFile-Functionality#autosave) bool to `DataFile`, which is on by default
- added [FileStyle](https://github.com/JimmyCushnie/SUCC/wiki/File-Style) class to control various aspects of how generated files are formatted. Each DataFile can have its own Style, and there is a global default Style.
- added [DoSaveAttribute](https://github.com/JimmyCushnie/SUCC/wiki/Custom-Complex-Type-Rules) for saving private fields and properties of complex types
- added [Utilities.LineEndingStyle](https://github.com/JimmyCushnie/SUCC/wiki/Utilities#lineendingstyle) to control how line endings are saved in the files
- [booleans](https://github.com/JimmyCushnie/SUCC/wiki/Base-Types#boolean) can now be loaded as "on"/"off", "yes"/"no", or "y"/"n" in addition to "true"/"false"
- some error messages improved
- no more restrictions on dictionary types
- explicit support for generic collections: T[], List\<T>, Dictionary<T1, T2>, HashSet\<T>
 - moved `Utilities.IndentationCount` to `FileStyle.IndentationInterval`
- renamed non-generic DataFile.Get and Set to GetNonGeneric and SetNonGeneric
- renamed non-generic DataFile.GetAsObject and SetAsObject to GetAsObjectNonGeneric and SetAsObjectNonGeneric
- an error will now be thrown if trying to set indentation interval to less than 1
- an error will now be thrown if you try to set [Utilities.DefaultPath](https://github.com/JimmyCushnie/SUCC/wiki/Utilities#defaultpath) to a path that is not absolute
- usage of the DontSave attribute is now properly restricted to fields and properties
- fixed issue where old data could stick around when new data was saved for collections
- fixed complex type shortcuts not being erased when a new value is saved
- fixed errors in Unity editor if you'd most recently compiled for webGL
- fixed various issues relating to multi-line strings
- fixed newly created data files beginning with an empty line
- fixed error if file is deleted on disk while program is running
- various performance improvements
- the main branch is now independent from Unity
- renamed the project from Sensible Unity/C# Configuration to Sensible and Utilitarian C# Configuration
- changed the license from MIT to WTFPL
- added new branches for development. The `master`, `unity`, and `unity-package-manager` branches will henceforth be kept at the latest stable release.

v0.3 | 2018-11-08
---

* renamed library to SUCC
* file extension of saved data is now .succ
* added constructor shortcuts
* added method shortcuts
* added support for WebGL
* improved the default file path and made it specific to different platforms - see README.md#platforms for details
* if trying to parse a custom type without all properties/fields defined, the undefined bits will now be left at their default value. Previously, an error would be thrown.
* added DataFile.GetTopLevelKeys()
* added DataFile.SaveAsObject<T> and DataFile.GetAsObject<T> as well as non-generic versions
* added non-generic Get and Set methods to DataFile
* DataFile.Get<T> method no longer requires a default value - if none is provided, it will use default(T)
* dictionaries can now be serialized and parsed properly
* added comment character escaping in compliance with PENIS spec 0.2
* added warning when trying to serialize null
* updated documentation

v0.2 | 2018-09-26
---

* added property shortcuts
* added System.Type as a base type which can be serialized/parsed
* will no longer save data to disk if the same data is already on the disk
* will now throw an exception if a file contains the tab character (this is invalid PENIS)
* relative files paths now work properly on non-windows OSs
* fixed being unable to parse List\<T>

v0.1 | 2018-09-22
---

* Initial release
