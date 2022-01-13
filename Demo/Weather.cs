using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BearMLLib.Serialization;

namespace Demo;
internal class Weather
{
    public TimeSpan Time { get; set; }

    [KeyAlias("temp")]
    [Comment("degree celsius.")]
    public double Temperature { get; set; }

    [IgnoreSerialization]
    public string Description { get; set; }
}
