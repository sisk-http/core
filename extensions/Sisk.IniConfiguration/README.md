# Sisk.IniConfiguration

A Sisk library for reading and writing INI files and using them as Sisk configurations.

Current implementation flavor:

- Properties and section names are **case-insensitive**.
- Properties names and values are **trimmed**.
- Values can be quoted with single or double quotes. Quotes can have line-breaks inside them.
- Comments are supported with `#` and `;`. Also, **trailing comments are allowed**.
- Properties can have multiple values.

## Usage

Using the following ini code as example:

```ini
One = 1
Value = this is an value
Another value = "this value
    has an line break on it"

; the code below has some colors
[some section]
Color = Red
Color = Blue
Color = Yellow ; do not use yellow
```

Parse it with:

```csharp
// parse the ini text from the string
IniDocument doc = IniDocument.FromString(iniText);

// get one value
string? one = doc.Global.GetOne("one");
string? anotherValue = doc.Global.GetOne("another value");

// get multiple values
string[]? colors = doc.GetSection("some section")?.GetMany("color");
```