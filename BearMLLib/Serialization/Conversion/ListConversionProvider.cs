using System;
using System.Collections.Generic;
using System.Text;
using BearMLLib.Text;

namespace BearMLLib.Serialization.Conversion
{
    internal class ListConversionProvider : IConversionProvider
    {
        private static readonly IConversionProvider _itemConversionProvider =
            new ConversionProvider(
                typeof(object),
                str => throw new NotImplementedException(),
                obj => new StringBuilder().Append(Identifier.FoldedListNode)
                                          .Append(TypeConverter.ConvertToString(obj).Escape(EscapeLevel.HL))
                                          .Append(Identifier.FoldedListNode)
                                          .ToString()
                );
        public Type Type { get => typeof(List<>); }

        public object ConvertFromString(string source)
        {
            throw new NotSupportedException();
        }

        public string ConvertToString(object source)
        {
            var sourceType = source.GetType();

            var sb = new StringBuilder();
            sb.Append(Identifier.FoldedListL);

            var count = (int)sourceType.GetProperty("Count").GetValue(source, new object[] { });

            if (count == 0) return sb.Append(Identifier.FoldedListR).ToString();

            for (var i = 0; i < count; i++)
            {
                var item = sourceType.GetProperty("Item").GetValue(source, new object[] { i });
                var value = TypeConverter.ConvertToString(item, new[] { this, _itemConversionProvider });
                sb.Append(value);

                sb.Append(Identifier.FoldedListSplit)
                  .Append(' ');
            }
            sb.Remove(sb.Length - 2, 2);

            sb.Append(Identifier.FoldedListR);

            return sb.ToString();
        }
    }
}
