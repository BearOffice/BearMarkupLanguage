using BearMarkupLanguage;
using BearMarkupLanguage.Core;
using Demo;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


var a = new BearML();
a.AddKeyValue("b", new Dictionary<string, List<string>> { { "123", new List<string> { "123", "1saf\nszag" } } });
a.SaveTo("x");

a = new BearML("x");
a.AddKeyValue("c", "c");