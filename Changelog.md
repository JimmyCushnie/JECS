# SUCC Changelog

v0.3 | 2018-11-08 (72b1aa0)
---

* renamed library to SUCC, since the language has substantially diverged from PENIS
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

v0.2 | 2018-09-26 (6cdfba4)
---

* added property shortcuts
* added System.Type as a base type which can be serialized/parsed
* will no longer save data to disk if the same data is already on the disk
* will now throw an exception if a file contains the tab character (this is invalid PENIS)
* relative files paths now work properly on non-windows OSs
* fixed being unable to parse List\<T>

v0.1 | 2018-09-22 (7f763fc)
---

* Initial release
