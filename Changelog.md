# SUCC Changelog

v0.3 | 2018-11-08
---

* renamed library to SUCC, since the language is now substantially different than PENIS
* file extension is now .succ
* added constructor shortcuts
* added method shortcuts
* added support for WebGL
* improved the default file path and made it specific to different platforms
* if trying to parse a custom type without all parameters defined, the undefined parameters will now be left at their default value. Previously, an error would be thrown.
* added DataFile.GetTopLevelKeys()
* added DataFile.SaveAsObject() and DataFile.GetAsObject()
* added non-generic Get and Set methods to DataFile
* DataFile.Get<T> method no longer requires a default value - if none is provided, it will use (default)T
* added comment escape characters in compliance with PENIS spec 0.2
* dictionaries can now be serialized and parsed properly
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
