<img src="https://raw.githubusercontent.com/JimmyCushnie/SUCC/master/Succ_Logo_nosubtitle.png" width = 480>

Sexy and Utilitarian C# Configuration  
Created by Jimmy Cushnie

---

Do your configuration files look like this?

```
<weapons>
    <weapon>
        <name>sword</name>
        <damage>10</damage>
        <attackSpeed>1</attackSpeed>
    </weapon>
    <weapon>
        <name>dagger</name>
        <damage>6</damage>
        <attackSpeed>1.3</attackSpeed>
    </weapon>
    <weapon>
        <name>axe</name>
        <damage>20</damage>
        <attackSpeed>0.4</attackSpeed>
    </weapon>
</weapons>
```

Do your configuration files, god forbid, look like this? <sup>cough cough<sup>easy save</sup></sup>

```
{"weapons":{"__type":"System.Collections.Generic.List`1[[Weapon, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]],mscorlib","value":[{"name":"sword","damage":10,"attackSpeed":1},{"name":"dagger","damage":6,"attackSpeed":1.3},{"name":"axe","damage":20,"attackSpeed":0.4}]}}
```

With SUCC, your configuration files can look like this:

```
weapons:
    -
        name: sword
        damage: 10
        attackSpeed: 1
    -
        name: dagger
        damage: 6
        attackSpeed: 1.3
    -
        name: axe
        damage: 20
        attackSpeed: 0.4
```

Look at that. That's a *beautiful* configuration file. There's zero wasted space on formatting. Everything is laid out clearly so that it's easy to find what you're looking for. The file is fast for a human to read and fast for a human to edit. If you were working on this game and needed to add a new weapon type, what would you rather the configuration file look like?

Furthermore, SUCC gives you a lot of freedom when writing or editing config files. You can play around with the colon position and the indentation level, you can add whitespace, you can add comments. The following file will, to SUCC, load exactly the same as the preceding one:

```
weapons:
    -
      # the sword is your starting weapon, very general purpose.
      name        : sword
      damage      : 10
      attackSpeed : 1
      
    -
      # daggers are useful against enemies with slow attack rates.
      name        : dagger
      damage      : 6
      attackSpeed : 1.3
      
    -
      # you use an axe when you need to get rid of a low-health
      # enemy as quickly as possible.
      name        : axe
      damage      : 20 # this is overpowered. TODO balance better
      attackSpeed : 0.4
```

Not only are SUCC files easy to work with in a text editor, they're easy to work with in your code too. Here is all the code required to recreate that configuration file:

```csharp
using System.Collections.Generic;
using SUCC;

public class Weapon
{
    public string name;
    public float damage;
    public float attackSpeed;
}

static class Program
{
    static void Main()
    {
        List<Weapon> weapons = new List<Weapon>()
        {
            new Weapon()
            {
                name = "sword",
                damage = 10,
                attackSpeed = 1
            },
            new Weapon()
            {
                name = "dagger",
                damage = 6,
                attackSpeed = 1.3f
            },
            new Weapon()
            {
                name = "axe",
                damage = 20,
                attackSpeed = 0.4f
            },
        };

        var file = new DataFile("weaponsFile");
        file.Set("weapons", weapons);
    }
}
```

The important part of that is

```csharp
var file = new DataFile("weaponsFile");
file.Set("weapons", weapons);
```

You keep a reference to that `DataFile` variable, and later when you need to read it, it's as simple as this:

```csharp
var weaponsList = file.Get<List<Weapon>>("weapons");
```

But it can be *even easier.* You just do

```csharp
var weaponsList = file.Get("weapons", defaultValue: weapons);
```

SUCC will check if a value called "weapons" exists in the file. If so, it will read the file and give you that data. If not, it will save `defaultValue` to the file and return it to you.

## Start Using SUCC

Much more information about SUCC can be found on the [wiki](https://github.com/JimmyCushnie/SUCC/wiki). If you're new to SUCC, you probably want to see the [Installing](https://github.com/JimmyCushnie/SUCC/wiki/Installing) and [Getting Started](https://github.com/JimmyCushnie/SUCC/wiki/Getting-Started) pages.

