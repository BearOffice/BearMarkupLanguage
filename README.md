# BearMarkupLanguage
A class library to read or modify BearML config file simply.  
BearML is an original markup language. Language detail: [Wiki](../../wiki/Bear-Markup-Language-ver-5.0)  

BearML version 5.0 <b>does not</b> compatible with version 3.0.
<br>

https://www.nuget.org/packages/BearMarkupLanguage

# How to use (part)
## Get values from config file
config.txt  
```
a literal value: @
  Hello.
  Goodbye.
|

# support key aliases
[obj|weather]
weather object: $
  Time: 12:10:05
  Temperature: 40.2

> block <
a tuple: 
  - apple
  - $
    price: $1.5
    color: red
  
  # support nested block
  > sub block <
  sub key: 1.234
```
```c#
using BearMarkupLanguage.Serialization;
using SerializableAttribute = BearMarkupLanguage.Serialization.SerializableAttribute;

[Serializable]
class Weather
{
    public TimeSpan Time { get; set; }

    public double Temperature { get; set; }

    [IgnoreSerialization]
    public string? Description { get; set; }
}
```
```c#
var ml = new BearML("config.txt");
var literal = ml.GetValue<string>("a literal value");
var weather = ml.GetValue<Weather>("weather");
var tuple = ml.GetValue<(string, Dictionary<string, string>)>("block", "a tuple");
var key = ml.GetValue<double>(new[] { "block", "sub block" }, "sub key");
```

## Change values
config before changes
```
value: 10.1
```
```c#
ml.ChangeValue("value", 20.6);
```
config after changes
```
value: 20.6
```

## Add values
```c#
ml.AddKeyValue("list", new List<double[]> 
{ 
    new[] { 1.1, 2.2 }, 
    new[] { 3.3, 4.4 } 
});
ml.AddKeyValue("dic", new Dictionary<int, Weather>
{
    {0, new Weather{Temperature = 12} },
    {1, new Weather{Temperature = 32, Time = new TimeSpan(12,10,15)} }
});
```
config file
```
list: 
  -
    - 1.1
    - 2.2
  -
    - 3.3
    - 4.4

dic: $
  0: $
    Time: 00:00:00
    Temperature: 12
  1: $
    Time: 12:10:15
    Temperature: 32
```

## Change config file's style
```c#
using BearMarkupLanguage.Core;

Format.PrintMode = PrintMode.Compact;
ml.FormatFile();
```
config file
```
list: 
  [["1.1", "2.2"], ["3.3", "4.4"]]

dic: 
  {"0": {"Time": "00:00:00", "Temperature": "12"}, "1": {"Time": "12:10:15", "Temperature": "32"}}
```
