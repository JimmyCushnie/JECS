# PENIS for Unity

PENIS for Unity is an implementation of [PENIS](https://github.com/JimmyCushnie/PENIS) for the Unity engine. With it, you can create configuration files for your game that are easy to read and modify.

Table Of Contents
---

- [Installing](#installing)
- [Usage](#usage)
- [Modifying Values in a File](#modifying-values-in-a-file)
- [Supported Types](#supported-types)
- [Other Useful Functions](#other-useful-functions)
- [FAQ](#faq)

Installing
---

Download the latest .unitypackage file from the [releases](https://github.com/JimmyCushnie/PENIS-for-Unity/releases/latest) page, then import it to Unity using `Assets -> Import Package -> Custom Package`. If you get errors after importing, go to `Edit -> Project Settings -> Player` and change Scripting Runtime Version to .NET 4.x.

Usage
---

If you haven't already, you should skim the [PENIS spec](https://github.com/JimmyCushnie/PENIS) so you understand the structure of a PENIS file.

First, create a new `DataFile` object, like so:

```csharp
using UnityEngine;
using PENIS;

public class Test : MonoBehaviour
{
    private void Start()
    {
        var file = new DataFile("testing/test1.PENIS", "test1file");
    }
}
```

Let's take a look at the two parameters we passed to the constructor.

The first, `"testing/test1.PENIS"`, is the **File Path**. This refers to the location of the file on disk. If you pass this parameter an absolute path, like `"C:/Files/File.PENIS"`, we'll be working with that exact file. However, if you don't specify a disk letter, the path will be relative to `Utilities.DefaultPath`. Unless you change it, the default path is [Application.persistentDataPath](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html). To check the absolute path of a file, get `DataFile.FilePath`.

The second parameter, `"test1file"`, is optional. That is the **Default File**. If you use this parameter and no file is found at File Path, the Default File will be copied there and used as a template. This is how you can have files in your game with all the lovely formatting that PENIS enables. The Default File must be a path in Unity's [Resources](https://docs.unity3d.com/ScriptReference/Resources.html) folder which refers to a [TextAsset](https://docs.unity3d.com/Manual/class-TextAsset.html), and not have a file extension.

When we run this code, the program will check if there is a file at `Utilities.DefaultPath + "/testing/test1.PENIS"`. If there isn't, it will create a new file and copy the data in `test1file` to it. Then, all the PENIS data in `test1.PENIS` will be loaded into the `file` variable for us to play with.

Modifying Values in a File
---

You can retrieve the value of a top-level key in the file using the `Get<T>` method. The following code will tell you what integer value is stored under the "number" key.

```csharp
private void Start()
{
    var file = new DataFile("testing/test1.PENIS", "test1file");
    int i = file.Get("number", 0);
    Debug.Log(i);
}
```

Note that we've passed *two* parameters in the `Get` method. The second item is the **Default Value**. If there does not exist a top-level key called `number` in the file, it will be created and set to the Default Value.

Note also that `Get` is a generic function. When you pass it a type, that type instructs it on how to interpret the data there.

The `Set` function works similarly; you give it a key, a value, and a type by which to serialize that value.

However, **`Get` and `Set` only modify the file in memory.** If you would like to save those changes to disk, you must call `SaveAllData`.

```csharp
private void Start()
{
    var file = new DataFile("testing/test1.PENIS");
    int i = file.Get("number", 0);
    Debug.Log(i);
    file.Set("number", i + 1);
    file.SaveAllData();
}
```

Every time you run the preceding code, the value in the file will increment by 1.

Supported Types
---

PENIS for Unity will serialize and parse the following base types according to the PENIS spec:

* string
* int
* decimal
* long
* short
* uint
* ulong
* ushort
* float
* double
* byte
* sbyte
* bool
* DateTime
* char
* enum
* System.Type (will use the type's name, including namespaces)

It can also serialize/parse lists and arrays.

If a different type is sent to the `Get` or `Set` methods, P4U will serialize all public, non-static, read-and-write-enabled fields and properties of that type, except for those with the `[PENIS.DontSave]` attribute. For example, the `Vector3` type will have the three floats `x` `y` and `z` serialized.

Other Useful Functions
---

* `DataFile.ReloadAllData()` reload the PENIS data of a `DataFile` from disk
* `DataFile.KeyExists(string key)` returns true if there exists a top-level key by that name in the file
* `DataFile.DeleteKey(string key)` removes a top-level key and all its data from a file
* `PENIS.Utilities.DefaultPath { get; set; }` allows you to change where relative file paths are relative to
* `PENIS.Utilities.IndentationCount { get; set; }` allows you to change the default indentation of data line children

FAQ
---

**Your code is bad.**

That's not a question, but you do have a point. Sorry.
