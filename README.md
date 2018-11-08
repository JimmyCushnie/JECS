# Sensible Unity/C# Configuration

SUCC is a tool for building configuration files files for your game. It has a simple, easy to use API, and the files it creates are easy to read and modify. SUCC is based on [PENIS](https://github.com/JimmyCushnie/PENIS), and it has all the lovely formatting that PENIS does; however, SUCC introduces some fancy tools specific to the C# language which make writing config files even nicer. This makes the SUCC language a superset of PENIS.

Although SUCC is built for the Unity engine, it should be relatively easy to adapt for any C# project.

Table Of Contents
---

- [Installing](#installing)
- [Usage](#usage)
    - [Creating a DataFile](#creating-a-datafile)
    - [Get and Set values in a file](#get-and-set-values-in-a-file)
- [Supported Types](#supported-types)
    - [Base Types](#base-types)
    - [Collections](#collections)
    - [Other Types](#other-types)
- [Shortcuts](#shortcuts)
    - [Property Shortcuts](#property-shortcuts)
    - [Constructor Shortcuts](#constructor-shortcuts)
    - [Method Shortcuts](#method-shortcuts)
- [Other Useful Functions](#other-useful-functions)
- [Platforms](#platforms)
- [The Future of SUCC](#the-future-of-succ)

Installing
---

Download the latest .unitypackage file from the [releases](https://github.com/JimmyCushnie/PENIS-for-Unity/releases/) page, then import it to Unity using `Assets -> Import Package -> Custom Package`. If you get errors after importing, go to `Edit -> Project Settings -> Player` and change Scripting Runtime Version to .NET 4.x.

Usage
---

If you haven't already, you should skim the [PENIS spec](https://github.com/JimmyCushnie/PENIS) so you understand the structure of a PENIS file.

### Creating a DataFile

To work with SUCC data, you must first create a new `DataFile` object:

```csharp
using UnityEngine;
using SUCC;

public class Test : MonoBehaviour
{
    private void Start()
    {
        var file = new DataFile("testing/test1.succ", "test1file");
    }
}
```

Let's take a look at the two parameters we passed to the constructor.

The first, `"testing/test1.succ"`, is the **File Path**. This refers to the location of the file on disk. If you pass this parameter an absolute path, like `"C:/Files/File.PENIS"`, we'll be working with that exact file. However, if you don't specify a disk letter, the path will be relative to `Utilities.DefaultPath`. For more details see the [Platforms]() section. To check the absolute path of a file, get `DataFile.FilePath`.

The second parameter, `"test1file"`, is optional. That is the **Default File**. If you use this parameter and no file is found at File Path, the Default File will be copied there and used as a template. The Default File must be a path in Unity's [Resources](https://docs.unity3d.com/ScriptReference/Resources.html) folder which refers to a [TextAsset](https://docs.unity3d.com/Manual/class-TextAsset.html). Do not pass a file extension when specifying a default file.

When we run this code, the program will check if there is a file at `Utilities.DefaultPath + "/testing/test1.succ"`. If there isn't, it will create a new file and copy the data in `test1file` to it. Then, all the formatted data in `test1.succ` will be loaded into the `file` variable for us to play with.

### Get and Set Values in a File

You can retrieve the value of a top-level key in the file using the `Get<T>` method. The following code will tell you what integer value is stored under the "number" key.

```csharp
private void Start()
{
    var file = new DataFile("testing/test1.succ", "test1file");
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

### Base Types

SUCC will serialize and parse the following base types according to [PENIS spec](https://github.com/JimmyCushnie/PENIS#string):

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

The final base type is `System.Type`. SUCC uses the type's name, including namespaces, to serialize and parse it.

### Collections

SUCC will serialize and parse List<T>s, arrays, and HashSet<T>s according to [PENIS spec](https://github.com/JimmyCushnie/PENIS#list-lines) as long as the collection is of a supported type.

SUCC can serialize and parse dictionaries according to [PENIS spec](https://github.com/JimmyCushnie/PENIS#key-lines) as long as the keys in the dictionary are of a base type and the values are of a supported type.

### Other Types

If a type not listed above is sent to the `Get` or `Set` methods, P4U will serialize all public, non-static, read-and-write-enabled fields and properties of that type, except for those with the `[SUCC.DontSave]` attribute. For example, the `Vector3` type will have the three floats `x` `y` and `z` serialized.

Shortcuts
---

Shortcuts are the main thing that separate SUCC language from PENIS. Shortcuts are never saved by SUCC; you use them when writing config files for your game.

### Property Shortcuts

If your type has a static, read-only propery that returns a value of that type, you can use it as a shortcut when writing configuration files. For example, SUCC will parse the following two lists of Vector3 exactly the same:

```
Vector List One:
    -
        x: 0
        y: 0
        z: 0
    -
        x: 0
        y: 1
        z: 0
    -
        x: 1
        y: 1
        z: 1

Vector List Two:
    - zero              # see https://docs.unity3d.com/ScriptReference/Vector3-zero.html
    - up                # see https://docs.unity3d.com/ScriptReference/Vector3-up.html
    - one               # see https://docs.unity3d.com/ScriptReference/Vector3-one.html
```

### Constructor Shortcuts

You can use the constructor of a type as a shortcut when writing it. To do this, separate the values you want to pass to the constructor with commas, and encase the value with parenthesis. The following list of Vector3s is parsed exactly the same as the two above:

```
Vector List Three:
    - (0, 0, 0)
    - (0, 1, 0)
    - (1, 1, 1)
```

Note that the parameters of constructors used in shortcuts must be of base types.

Constructor shortcuts are a little finicky, because a type can have multiple constructors. SUCC uses the following rules to determine which constructor to use with a constructor shortcut:

- if the type has only one constructor, that constructor is used
- if the type has more than one constructor, the first constructor with the same number of parameters as those used in the shortcut is used. If no constructors exist with a matching parameter length, the first constructor is used.

### Method Shortcuts

If your type has a static method which returns a value of that type, you can use it as a shortcut and pass it parameters in the same way as constructor shortcuts. Consider the following class:

```csharp
public class Example
{
    public List<int> Numbers = new List<int>();
    public List<string> Words = new List<string>();

    public static Example Sequence(int count)
    {
        var example = new Example();
        for (int i = 0; i < count; i++)
        {
            example.Numbers.Add(i);
            example.Words.Add(i.ToString());
        }
        return example;
    }
}
```

The following two values will parse the exact same value when interpreted as `Example`:

```
Item1:
    Numbers:
        - 0
        - 1
        - 2
        - 3
        - 4
    Words:
        - "0"
        - "1"
        - "2"
        - "3"
        - "4"

Item2: Sequence(5)

```

Other Useful Functions
---

* `DataFile.ReloadAllData()` reload the data of a `DataFile` from disk
* `DataFile.KeyExists(string key)` returns true if there exists a top-level key by that name in the file
* `DataFile.DeleteKey(string key)` removes a top-level key and all its data from a file
* `SUCC.Utilities.DefaultPath { get; set; }` allows you to set your own default path where files are saved and loaded from if you don't like the default one.
* `SUCC.Utilities.IndentationCount { get; set; }` allows you to change the default indentation of data line children

Platforms
---

Currently, SUCC supports the exact same API on Windows, Linux, Mac OS, and WebGL. Here are the differences on each platform:

* **Desktop (Windows, Linux, Mac OS)**: The default file path is the same folder that the game executable is in.
* **Unity Editor**: The default file path is a folder called `Game/` which is in the same parent directory as your `Assets` folder. You'll probably want to add the `Game/` folder to your gitignore.
* **WebGL**: On web, you don't really have access to a file system. SUCC uses [IndexedDB](https://en.wikipedia.org/wiki/Indexed_Database_API) via [PlayerPrefs](https://docs.unity3d.com/ScriptReference/PlayerPrefs.html) to store its files.

The Future of SUCC
---

Before 1.0 release, I want to:
- clean up the code, in particular the parsing logic
- add more helpful error messages when the parser encounters invalid data
- add example scenes and code to demonstrate how SUCC is used
