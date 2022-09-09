using System;
using BearMarkupLanguage.Serialization;
using SerializableAttribute = BearMarkupLanguage.Serialization.SerializableAttribute;

namespace Demo;

[Serializable]
class Weather
{
    public TimeSpan Time { get; set; }

    public double Temperature { get; set; }

    [IgnoreSerialization]
    public string? Description { get; set; }
}