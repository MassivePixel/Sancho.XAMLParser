// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace Sancho.DOM.XamarinForms
{
    [Flags]
    public enum BindableContextAttributes
    {
        IsManuallySet = 1 << 0,
        IsBeingSet = 1 << 1,
        IsDynamicResource = 1 << 2,
        IsSetFromStyle = 1 << 3,
        IsDefaultValue = 1 << 4
    }

    public class BindablePropertyContext
    {
        public BindableContextAttributes Attributes;
        public BindingBase Binding;
        public BindableProperty Property;
        public object Value;
    }

    public static class BindableObjectHelper
    {
        public static List<BindablePropertyContext> GetProperties(this BindableObject bo)
        {
            if (bo == null)
                throw new ArgumentNullException(nameof(bo));

            var list = new List<BindablePropertyContext>();

            var props = typeof(BindableObject)
                .GetTypeInfo()
                .GetDeclaredField("_properties")
                .GetValue(bo)
                as IEnumerable;

            if (props != null)
            {
                Type propType = null;

                foreach (var prop in props)
                {
                    propType = propType ?? prop.GetType();
                    var p = propType.GetRuntimeField("Property")?.GetValue(prop) as BindableProperty;
                    var v = propType.GetRuntimeField("Value")?.GetValue(prop);
                    var b = propType.GetRuntimeField("Binding")?.GetValue(prop) as BindingBase;
                    var a = (int)propType.GetRuntimeField("Attributes")?.GetValue(prop);
                    list.Add(new BindablePropertyContext
                    {
                        Property = p,
                        Value = v,
                        Binding = b,
                        Attributes = (BindableContextAttributes)a
                    });
                }
            }

            return list;
        }
    }
}
