using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BearMarkupLanguage.Elements.Helpers;

internal class BlockElementContainer : IElementContainer<Block>
{
    public TaggedLine OriginTaggedLine { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public Block Element { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
}
