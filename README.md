# JECS

Jimmy's Epic Config System

---

JECS is a C# library for reading and writing configuration files. It is focused on ease of use: the API is simple and intuitive, and the generated files are likewise straightforward, readable, and easy to manually tweak.

JECS makes it easy for you to expose data in your program through human-readable and human-editable plaintext files. We use it extensively in [Logic World](https://store.steampowered.com/app/1054340/Logic_World/) for the game's user settings, save files, modding system, and more.

## Basic usage

Create a `DataFile` object, which corresponds to an actual `.jecs` file on disk. Use the `Get()` and `Set()` methods to get and set data in the file.

```csharp
var userSettingsFile = new DataFile(path: "user_settings.jecs");

// Gets the string stored in user_settings.jecs under the key `user_name`.
// If no key by that name exists, it will be created set to `Spongebob Squarepants`.
string userName = userSettingsFile.Get(key: "user_name", defaultValue: "Spongebob Squarepants");

// Outputs the value stored in the file, or "Spongebob Squarepants" if the value/file didn't exist yet.
Console.WriteLine(userName);
```

The above code will create a file called `user_settings.jecs` with the following contents:

```yaml
user_name: Spongebob Squarepants
```

## Intermediate usage

JECS cleanly handles complex nested data structures. You can feed JECS most kinds of data, and that data will be saved and loaded as expected.

```csharp
class RepeatingReminder
{
    public string ReminderText;
    public DayOfWeek Day;
    public BigInteger Importance;
}

static void Main()
{
    var reminders = new RepeatingReminder[]
    {
        new RepeatingReminder()
        {
            ReminderText = "Water plants",
            Day = DayOfWeek.Thursday,
            Importance = 100,
        },
        new RepeatingReminder()
        {
            ReminderText = "Check mail",
            Day = DayOfWeek.Thursday,
            Importance = 75,
        },
        new RepeatingReminder()
        {
            ReminderText = "Have a shower",
            Day = DayOfWeek.Monday,
            Importance = BigInteger.Pow(9, 40),
        },
    };

    var remindersFile = new DataFile("reminders.jecs");
    remindersFile.Set(key: "reminders", value: reminders);
}
```

The above code will create will create a file called `reminders.jecs` with the following contents:

```yaml
reminders:
    -
        ReminderText: Water plants
        Day: Thursday
        Importance: 100
    -
        ReminderText: Check mail
        Day: Thursday
        Importance: 75
    -
        ReminderText: Have a shower
        Day: Monday
        Importance: 147808829414345923316083210206383297601
```

This is a great example of how nice JECS files are. Notice how this config file has everything laid out clearly so that it's easy to find what you're looking for. The file has minimal formatting boilerplate, so that almost everything in the file is the actual data. This file is fast for a human to read, and fast for a human to edit or expand.

Furthermore, JECS gives you a lot of freedom when writing or editing config files. You can add or remove whitespace, use a different indentation level, or add comments. For example:

```yaml
reminders:
    -
        ReminderText  :  Water plants
        Day           :  Thursday
        Importance    :  100
    
    -
        ReminderText  :  Check mail
        Day           :  Wednesday
        Importance    :  75 # Don't change this. Critical that it's exactly 75
    
    -
        ReminderText  :  Have a shower
        Day           :  Monday
        Importance    :  147808829414345923316083210206383297601

# Todo: add reminders about skydiving
```

For more information on the file format, see the [File Structure wiki page](https://github.com/JimmyCushnie/JECS/wiki/File-Structure).

## Advanced usage

JECS has many powerful features that make working with it, both from code and in the config files themselves, smooth and easy. This includes [custom base types], [control over the format of generated files], [auto-reloading when a file changes on disk], and much more.

---

For full documentation on all of JECS's features, and a guide to installing + getting started, see the [wiki](https://github.com/JimmyCushnie/JECS/wiki).

