﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BearMarkupLanguage.Elements.Helpers;

internal interface IElementContainer<T>
{
    public TaggedLine OriginTaggedLine { get; init; }
    public T Element { get; init; }
}
