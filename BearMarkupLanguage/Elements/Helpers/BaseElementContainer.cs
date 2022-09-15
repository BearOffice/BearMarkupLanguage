using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BearMarkupLanguage.Elements.Helpers;

internal class BaseElementContainer : IElementContainer<IBaseElement>
{
    public TaggedLine OriginTaggedLine { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public IBaseElement Element { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
}
