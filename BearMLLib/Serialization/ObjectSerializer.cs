using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using BearMLLib.Configs;
using BearMLLib.Core;
using BearMLLib.Serialization.Conversion;

namespace BearMLLib.Serialization
{
    internal static class ObjectSerializer
    {
        internal static object Deserialize(KeyContentPair[] pairs, Type targetType, IConversionProvider[] providers)
        {
            var target = Activator.CreateInstance(targetType);

            var pairsList = pairs.ToList();

            foreach (var field in targetType.GetFields())
            {
                if (HasIgnoreAttribute(field)) continue;

                var name = field.Name;
                var type = field.FieldType;

                var pair = GetAndRemoveKeyContentPair(name);

                field.SetValue(target, pair.Content.Get(type, providers));
            }

            foreach (var prop in targetType.GetProperties())
            {
                if (HasIgnoreAttribute(prop)) continue;

                var name = prop.Name;
                var type = prop.PropertyType;

                var pair = GetAndRemoveKeyContentPair(name);

                prop.SetValue(target, pair.Content.Get(type, providers));
            }

            if (pairsList.Count > 0)
                throw new InvalidConfigException("Configs are inconsistent with the given type.");

            return target;

            KeyContentPair GetAndRemoveKeyContentPair(string key)
            {
                foreach (var pair in pairsList.ToArray())
                {
                    if (pair.Key.Name == key || pair.Key.AliasName == key)
                    {
                        pairsList.Remove(pair);
                        return pair;
                    }
                }

                throw new InvalidConfigException($"Missing config. The config {key} is not exists.");
            }
        }

        internal static T Deserialize<T>(KeyContentPair[] pairs, IConversionProvider[] providers)
        {
            return (T)Deserialize(pairs, typeof(T), providers);
        }

        internal static KeyContentPair[] Serialize(object source, Type sourceType, IConversionProvider[] providers)
        {
            var pairs = new List<KeyContentPair>();

            foreach (var field in sourceType.GetFields())
            {
                if (HasIgnoreAttribute(field)) continue;

                var key = field.Name;
                var content = ContentTypeSelector.GetContent(field.GetValue(source), providers);
                pairs.Add(new KeyContentPair(new Key(key, null, null), content));
            }

            foreach (var property in sourceType.GetProperties())
            {
                if (HasIgnoreAttribute(property)) continue;

                var key = property.Name;
                var content = ContentTypeSelector.GetContent(property.GetValue(source), providers);
                pairs.Add(new KeyContentPair(new Key(key, null, null), content));
            }

            return pairs.ToArray();
        }

        internal static KeyContentPair[] Serialize<T>(T source, IConversionProvider[] providers)
        {
            return Serialize(source, typeof(T), providers);
        }

        private static bool HasIgnoreAttribute(PropertyInfo prop)
        {
            foreach (var attr in Attribute.GetCustomAttributes(prop))
            {
                if (attr is IgnoreSerializationAttribute)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasIgnoreAttribute(FieldInfo field)
        {
            foreach (var attr in Attribute.GetCustomAttributes(field))
            {
                if (attr is IgnoreSerializationAttribute)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
