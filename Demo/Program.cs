using System;
using System.Collections.Generic;
using System.Text;
using BearMLLib;
using BearMLLib.Core;
using BearMLLib.Helpers;

namespace Demo;
class Program
{
    static void Main()
    {
        var bearML = new BearML(@"config.txt");
        Format.This.ValueMode = ValueMode.ForceEscaped;
        Console.WriteLine("int is {0}", bearML.GetContent<string>("int"));
        Console.WriteLine("double is {0}", bearML.GetContent<double>("double"));
        Console.WriteLine("time is {0}", bearML.GetContent<TimeSpan>("time"));

        Console.WriteLine();

        Console.WriteLine("literial's comment is '{0}'", bearML.GetComment("literial"));
        Console.WriteLine("literial's key alias is '{0}'", bearML.GetKeyAlias("literial"));
        Console.WriteLine("literial is:\n{0}", bearML.GetContent<string>("li"));

        Console.WriteLine();

        Console.WriteLine("escaped is:\n{0}", bearML.GetContent<string>("escaped"));

        Console.WriteLine();

        Console.WriteLine("expanded is {0}",
            ListVisualizer.Visualize(bearML.GetContent<List<List<string>>>("list group", "expanded")));
        Console.WriteLine("folded is {0}",
            ListVisualizer.Visualize(bearML.GetContent<List<List<string>>>("list group", "folded")));
        Console.WriteLine("mixed is {0}",
            ListVisualizer.Visualize(bearML.GetContent<List<List<string>>>("list group", "mixed")));

        Console.WriteLine();

        var weatherObj = bearML.DeserializeObjectGroup<Weather>("weather obj");
        Console.WriteLine("Temp is {0} degree. Time is {1}.", weatherObj.Temperature, weatherObj.Time);
        weatherObj = new Weather() { Time = new TimeSpan(10, 11, 12), Temperature = 22 };
        bearML.ChangeObjectGroup("weather obj", weatherObj);
        Console.WriteLine("Temp is {0} degree. Time is {1}.", weatherObj.Temperature, weatherObj.Time);
    }
}