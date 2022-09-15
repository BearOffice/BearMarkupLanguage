using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BearMarkupLanguage.Serialization;

/// <summary>
/// Indicates a field or property that should not be serialized or deserialized.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class IgnoreSerializationAttribute : Attribute { }
