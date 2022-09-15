using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BearMarkupLanguage.Serialization;

/// <summary>
/// Indicates a class or struct that can be serialized or deserialized.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class SerializableAttribute : Attribute { }