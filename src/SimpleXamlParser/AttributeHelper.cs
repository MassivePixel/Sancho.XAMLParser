// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace SimpleXamlParser
{
    public static class AttributeHelper
    {
        public static bool Apply(object parent, PropertyInfo prop, string value)
        {
            if (value.StartsWith("{"))
            {
                // handle markup extensions
                return ParseMarkupExtension(parent, prop, value);
            }
            else
            {
                // regular attributes are value
                object parsedValue;
                if (Parse(prop.PropertyType, value, out parsedValue) &&
                    prop.CanWrite)
                {
                    prop.SetValue(parent, parsedValue);
                    return true;
                }
            }

            return false;
        }

        public static bool Parse(Type propertyType, string attributeValue, out object value)
        {
            value = null;

            if (propertyType == typeof(string))
            {
                value = attributeValue;
                return true;
            }

            if (propertyType.GetTypeInfo().IsEnum)
            {
                var isDefined = Enum.IsDefined(propertyType, attributeValue);
                if (isDefined)
                {
                    value = Enum.Parse(propertyType, attributeValue);
                    return true;
                }

                return false;
            }

            if (propertyType == typeof(Int32))
            {
                Int32 i;
                if (Int32.TryParse(attributeValue, out i))
                {
                    value = i;
                    return true;
                }

                return false;
            }

            if (propertyType == typeof(double))
            {
                double d;
                if (double.TryParse(attributeValue, out d))
                {
                    value = d;
                    return true;
                }

                return false;
            }

            if (propertyType == typeof(bool))
            {
                bool b;
                if (bool.TryParse(attributeValue, out b))
                {
                    value = b;
                    return true;
                }

                return false;
            }

            var typeConverter = ReflectionHelpers.GetTypeConverter(propertyType);
            if (typeConverter == null)
            {
                var typeConverterAttr = propertyType.GetTypeInfo().GetCustomAttribute<TypeConverterAttribute>();
                if (typeConverterAttr != null)
                {
                    ReflectionHelpers.AddTypeConverter(propertyType, typeConverterAttr.ConverterTypeName);
                    typeConverter = ReflectionHelpers.GetTypeConverter(propertyType);
                }
            }

            if (typeConverter != null)
            {
                value = typeConverter.ConvertFromInvariantString(attributeValue);
                return true;
            }

            return false;
        }

        public static bool ParseMarkupExtension(object parent, PropertyInfo prop, string value)
        {
            if (value.StartsWith("{") &&
                value.EndsWith("}"))
            {
                value = value.Trim(new[] { '{', '}' });
            }

            var firstSpace = value.IndexOf(' ');
            var extension = firstSpace == -1 ? value : value.Substring(0, firstSpace);
            var rest = firstSpace == -1 ? string.Empty : value.Substring(firstSpace + 1);
            if (extension == "Binding")
                return ParseBinding(parent as BindableObject, prop, rest);

            return false;
        }

        public static bool ParseBinding(BindableObject bo, PropertyInfo prop, string rest)
        {
            var targetProperty = bo.GetType()
                                   .GetRuntimeFields()
                                   .Select(field => field.GetValue(null))
                                   .OfType<BindableProperty>()
                                   .FirstOrDefault(bp => bp.PropertyName == prop.Name);

            if (targetProperty != null)
            {
                var binding = string.IsNullOrWhiteSpace(rest) ? new Binding(".") : new Binding(rest);
                bo.SetBinding(targetProperty, binding);

                return true;
            }

            return false;
        }
    }
}
