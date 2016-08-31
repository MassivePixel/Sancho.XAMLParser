// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleXamlParser.Interfaces;
using Xamarin.Forms;

namespace SimpleXamlParser
{
    public class XamlDOMCreator : IXamlDOM
    {
        public object Root { get; private set; }
        public Action<string> Logger { get; set; }

        public TargetPlatform Platform { get; set; }

        public XamlDOMCreator()
        {
            Logger = s => { };
        }

        public void AddNode(XamlNode node)
        {
            if (node == null) return;

            AddChild(null, node);
        }

        public void AddChild(object parent, XamlNode node)
        {
            var element = CreateNode(node);

            if (parent == null)
            {
                Root = element;
            }
            else if (element is View)
            {
                AddToParent(parent, (View)element);
            }
        }

        public void AddToParent(object parent, View view)
        {
            if (parent == null || view == null)
                return;

            var contentProp = ReflectionHelpers.GetContentProperty(parent.GetType());
            if (contentProp != null)
            {
                if (typeof(IList<View>).GetTypeInfo().IsAssignableFrom(contentProp.PropertyType.GetTypeInfo()))
                {
                    var list = contentProp.GetValue(parent) as IList<View>;
                    list?.Add((View)view);
                }
                else if (typeof(View).GetTypeInfo().IsAssignableFrom(contentProp.PropertyType.GetTypeInfo()))
                {
                    contentProp.SetValue(parent, (View)view);
                }
            }
        }

        public void ApplyProperty(object parent, XamlProperty xamlProperty)
        {
            if (string.IsNullOrWhiteSpace(xamlProperty.Name))
            {
                if (xamlProperty.Value is string)
                {
                    var contentProp = ReflectionHelpers.GetContentProperty(parent.GetType());
                    if (contentProp != null &&
                        contentProp.PropertyType == typeof(string))
                    {
                        contentProp.SetValue(parent, xamlProperty.Value);
                    }
                }
                else if (xamlProperty.Value is XamlNode)
                {
                    var view = CreateNode((XamlNode)xamlProperty.Value) as View;
                    if (view != null)
                        AddToParent(parent, view);
                }
                else if (xamlProperty.Value is XamlNodeCollection)
                {
                    foreach (var node in ((XamlNodeCollection)xamlProperty.Value).Nodes)
                    {
                        var view = CreateNode((XamlNode)xamlProperty.Value) as View;
                        if (view != null)
                            AddToParent(parent, view);
                    }
                }
            }
            else if (xamlProperty.Name.Contains("."))
            {
                HandleAttachedProperty(parent as VisualElement, xamlProperty.Name, xamlProperty.Value as string);
            }
            else
            {
                var prop = parent.GetType().GetRuntimeProperty(xamlProperty.Name);
                if (prop != null)
                {
                    if (xamlProperty.Value is string)
                    {
                        AttributeHelper.Apply(parent, prop, (string)xamlProperty.Value);
                    }
                    else if (xamlProperty.Value is XamlNodeCollection)
                    {
                        var values = ((XamlNodeCollection)xamlProperty.Value).Nodes
                                                                             .Select(node => CreateNode(node))
                                                                             .Where(v => v != null)
                                                                             .ToList();

                        if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(prop.PropertyType.GetTypeInfo()))
                        {
                            var addMethod = prop.PropertyType
                                                .GetRuntimeMethods()
                                                .FirstOrDefault(m => m.Name == "Add");

                            var propValue = prop.GetValue(parent);
                            if (propValue == null)
                            {
                                try
                                {
                                    prop.SetValue(parent, propValue = Activator.CreateInstance(prop.PropertyType));
                                }
                                catch { }
                            }

                            if (propValue != null && addMethod != null)
                            {
                                foreach (var value in values)
                                    addMethod.Invoke(propValue, new[] { value });
                            }
                        }
                        else if (values.Any())
                        {
                            var value = values.First();

                            if (prop.PropertyType.GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo()))
                            {
                                prop.SetValue(parent, values[0]);
                            }
                            else if (value?.GetType()?.Name?.StartsWith("OnPlatform") == true)
                            {
                                if (Platform == TargetPlatform.iOS)
                                {
                                    var platprop = value.GetType()
                                                        .GetRuntimeProperty(nameof(OnPlatform<int>.iOS));

                                    if (platprop != null)
                                    {
                                        var platformValue = platprop.GetValue(value);
                                        prop.SetValue(parent, platformValue);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public object CreateNode(XamlNode node)
        {
            if (node.Name == "DataTemplate")
            {
                return new DataTemplate(() =>
                {
                    if (node.Children.Any())
                        return CreateNode(node.Children.FirstOrDefault());
                    return null;
                });
            }

            var type = ReflectionHelpers.GetType(node.Name);
            if (type != null)
            {
                var element = Activator.CreateInstance(type) as VisualElement;

                foreach (var child in node.Children)
                    AddChild(element, child);

                foreach (var prop in node.Properties)
                    ApplyProperty(element, prop);

                return element;
            }

            type = ReflectionHelpers.GetAllType(node.Name);
            if (type != null)
            {
                var element = Activator.CreateInstance(type);

                foreach (var child in node.Children)
                    AddChild(element, child);

                foreach (var prop in node.Properties)
                    ApplyProperty(element, prop);

                return element;
            }

            if (node.Name == "OnPlatform")
            {
                type = ReflectionHelpers.AllTypes.FirstOrDefault(t => t.Key.Contains("OnPlatform")).Value;

                var typeArgumentProperty = node.Properties.FirstOrDefault(p => p.Name == "TypeArguments");
                if (typeArgumentProperty != null)
                {
                    var typeArgument = ReflectionHelpers.GetType(typeArgumentProperty.Value as string) ??
                                       ReflectionHelpers.GetAllType(typeArgumentProperty.Value as string);
                    if (typeArgument != null)
                    {
                        type = type.MakeGenericType(new[] { typeArgument });
                        var element = Activator.CreateInstance(type);

                        foreach (var prop in node.Properties)
                        {
                            ApplyProperty(element, prop);
                        }

                        return element;
                    }
                }
            }

            return null;
        }

        public void HandleAttachedProperty(VisualElement view, string property, string value)
        {
            var parts = property.Split(new[] { '.' });

            if (parts.Length != 2)
            {
                Logger($"Invalid attached property '{property}'");
            }

            Type owner = ReflectionHelpers.GetType(parts[0]);
            if (owner == null)
            {
                Logger($"Owner type for attached property '{parts[0]}' not found");
                return;
            }

            var methods = owner.GetTypeInfo().DeclaredMethods;
            var method = methods.FirstOrDefault(x => x.Name == $"Set{parts[1]}");
            if (method == null)
            {
                Logger($"Cannot find setter method for attached property {parts[1]} on type {parts[0]}");
                return;
            }

            var parameters = method.GetParameters();

            object converted;
            if (AttributeHelper.Parse(parameters[1].ParameterType, value, out converted))
                method.Invoke(null, new object[] { view, converted });
            else
                Logger($"Cannot parse value '{value}' for attached property {property}");
        }
    }
}
