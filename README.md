# BearMarkupLanguageLib
A class library to get or save configs simply and reliably.  
This library uses an original markup language.  
The language's detail: [Wiki](../../wiki/Bear-Markup-Language)
<br>

# How to use
## Get content from config file.
```
> config.txt
# a
# number of pi
[num]
number: 3.1415

escaped str: Hello\nGoodbye.

[li]
literial str: |
  Hello
    Goodbye.
  <- EOL
        <- EOL
<- EOL

>>> list group
[fl]
folded list: >
  [["Today\nis", "not"], ["monday."]]

[el]
expanded list: >
  - >
    - |
      Today
      is not
  - > 
    - monday.

[ml]
mixed list: >
  - >
    ["Today", "is"]
  - > 
    ["friday."]
```
```C#
using BearMLLib;

BearML reader = new BearML("config.txt");
double num1 = reader.Get<double>("default", "number");
double num2 = reader.Get<double>("number");
double aliasName = reader.GetComment<double>("number");
double num3 = reader.Get<double>("num");
double comment = reader.GetComment<double>("num");
```
```
> result
num1        = 3.1415
num2        = 3.1415
aliasName   = num
num3        = 3.1415
comment     = @" a
 number of pi"
```
<br>

```C#
string escaped = reader.Get<string>("escaped");
string literial = reader.Get<string>("li");
```
```
> result
escaped     = @"Hello
Goodbye"
literial    = @"Hello
Goodbye.
      "
```
<br>

```C#
list1 = reader.Get<List<List<string>>>("list group", "fl");
list2 = reader.Get<List<List<string>>>("list group", "el");
list3 = reader.Get<List<List<string>>>("list group", "ml");
```
```
> result
list1[0][0] = @"Today
is"
list1[0][1] = "not"
list1[1][0] = "monday."

list2[0][0] = @"Today
is not"
list2[1][0] = "monday."

list3[0][0] = "Today"
list3[0][1] = "is"
list3[1][0] = "friday."
```
<br>

## Serialize object to config file.
```C#
class Weather
{
    public TimeSpan Time { get; set; }

    public double Temperature { get; set; }

    [IgnoreSerialization]
    public string Description { get; set; }
}
```
```C#
Weather weatherObj = new Weather{
    Time = new TimeSpan(12, 00, 00),
    Temperature = 18.2,
    Description = "A good weather"
};
reader.AddObjectGroup<Weather>("weather obj", weatherObj);
```
```
> result
> config.txt
>>> weather obj
Time: 12:00:00
Temperature: 18.2
```
<br>

```C#
weatherObj.Temperature = -12.1;
reader.ChangeObjectGroup<Weather>("weather obj", weatherObj);
```
```
> result
> config.txt
>>> weather obj
Time: 12:00:00
Temperature: -12.1
```

<br>

## Deserialize object from config file.
```
> config.txt
>>> weather obj
Time: 12:00:00
Temperature: 18.2
```
```C#
Weather weatherObj = reader.DeserializeObjectGroup<Weather>("weather obj");
```
```
> result
weatherObj.Time        = 12:00:00
weatherObj.Temperature = 18.2
weatherObj.Description = null
```
<br>

## Change or set content to config file.
```
> config.txt
>>> numbers
[num]
number: 3.1415
```
```C#
reader.ChangeContent<string>("numbers", "num", "2.71828??");
reader.AddKey<List<int>>("numbers", "number1", "num1", null, new List<int> { 1, 2, 3 });
reader.AddKey<double>("number0", 0.001);
```
```
> result
> config.txt
number0: 0.001
>>> numbers
[num]
number: 2.71828??
[num1]
number1: >
  - 1
  - 2
  - 3
```
<br>

## Change config file's style
```
> config.txt
str: |
  Hello
    Good Morning
list: >
  - >
    - 1
    - 2
    - 3
    - 4
  - >
    - 5
```
```C#
using BearMLLib.Core;

Format.This.ValueMode = ValueMode.ForceEscaped;
Format.This.ListMode = ListMode.ForceFolded;
reader.Save();
```
```
> result
> config.txt
str: Hello\n  Good Morning
list: >
  [["1", "2", "3", "4"], ["5"]]
```
<br>

```C#
Format.This.ListMode = ListMode.Auto;
Format.This.MaximumElementNumber = 3;
reader.Save();
```
```
> result
str: Hello\n  Good Morning
list: >
  - >
    ["1", "2", "3", "4"]
  - >
    - 5
```
<br>

## Define new serializable types

```C#
using BearMLLib.Serialization.Conversion;

var provider = new ConversionProvider(
    typeof(ValueTuple<int, int>),
    str =>
    {
        var s = str[1..^1].Split(',');
        var left = s[0];
        var right = s[1];
        return (int.Parse(left), int.Parse(right));
    },
    obj =>
    {
        var tuple = ((int, int))obj;
        return "(" + tuple.Item1 + "," + tuple.Item2 + ")";
    }
);

var reader = new BearML("config.txt", new[] { provider });
var tupleList = new List<ValueTuple<int, int>>{
    (1, 2),
    (3, 4),
};

reader.AddKey("tuple list", tupleList);
```
```
> result
> config.txt
tuple list: >
  - (1,2)
  - (3,4)
```
<br>

```C#
var tupleListObj = reader.GetContent<List<ValueTuple<int, int>>>("tuple list");
```
```
> result
tupleListObj[0] = (1, 2)
tupleListObj[1] = (3, 4)
```
