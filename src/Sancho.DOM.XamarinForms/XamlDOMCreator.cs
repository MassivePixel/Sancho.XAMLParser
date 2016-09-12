// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sancho.XAMLParser;
using Sancho.XAMLParser.Interfaces;
using Serilog;
using Xamarin.Forms;

namespace Sancho.DOM.XamarinForms
{
    public partial class XamlDOMCreator : IXamlDOM
    {
        IXamlServices xamlServices;

        public object Root { get; private set; }
        public TargetPlatform Platform { get; set; }

        Dictionary<XamlNode, object> nodeToElementMapper = new Dictionary<XamlNode, object>();

        public XamlDOMCreator()
        {
            Log.Verbose($"{nameof(XamlDOMCreator)} created");
        }

        public XamlDOMCreator(IXamlServices services)
        {
            xamlServices = services;

            Log.Verbose($"{nameof(XamlDOMCreator)} created");
        }

        public void AddNode(XamlNode node)
        {
            Log.Verbose("Adding node {node}", node);

            if (node == null)
            {
                Log.Warning("Cannot add null node");
                return;
            }

            Root = null;
            nodeToElementMapper = new Dictionary<XamlNode, object>();

            AddChild(null, node);
            if (Root != null)
            {
                // now apply markup extensions
                ApplyMarkupExtensions(nodeToElementMapper.FirstOrDefault(x => x.Value == Root).Key);
            }
        }

        void ApplyMarkupExtensions(XamlNode node, XamlNode parent = null)
        {
            if (node == null) return;

            foreach (var child in node.Children)
                ApplyMarkupExtensions(child, node);

            foreach (var prop in node.Properties)
            {
                if (!(prop is XamlStringProperty))
                    continue;

                var value = prop.GetString();
                if (value.StartsWith("{", StringComparison.Ordinal))
                {
                    var element = nodeToElementMapper[node];
                    var p = element.GetType().GetRuntimeProperty(prop.Name);
                    if (p == null)
                    {
                        Log.Warning($"No property {prop.Name} found under {element.GetType().FullName}");
                        return;
                    }

                    ParseMarkupExtension(element, p, value);
                }
            }
        }

        void AddChild(object parent, XamlNode node)
        {
            var element = CreateNode(node, parent);

            if (parent == null)
            {
                Log.Debug("Setting {node} as root", element);
                Root = element;
            }
            else if (element is View)
            {
                Log.Debug("Adding {element} as child to parent", element);
                AddToParent(parent, (View)element);
            }
            else if (element is Setter)
            {
                if (parent is Style)
                    ((Style)parent).Setters.Add((Setter)element);
            }
            else if (element is ResourceDictionary)
            {
                if (parent is VisualElement)
                {
                    Log.Debug("Adding resource dictionary to parent page");
                    ((VisualElement)parent).Resources = (ResourceDictionary)element;
                }
            }
        }

        void AddToParent(object parent, View view)
        {
            if (parent == null || view == null)
            {
                Log.Warning("Both parent and view must be non null");
                return;
            }

            var contentProp = ReflectionHelpers.GetContentProperty(parent.GetType());
            if (contentProp != null)
            {
                Log.Debug($"Content property {contentProp.Name} found for {{parent}}", parent);

                if (typeof(IList<View>).GetTypeInfo().IsAssignableFrom(contentProp.PropertyType.GetTypeInfo()))
                {
                    Log.Debug("Content property is a list");
                    var list = contentProp.GetValue(parent) as IList<View>;
                    list?.Add(view);
                }
                else if (typeof(View).GetTypeInfo().IsAssignableFrom(contentProp.PropertyType.GetTypeInfo()))
                {
                    Log.Debug("Content property is a single View");
                    contentProp.SetValue(parent, view);
                }
                else
                {
                    Log.Warning($"Unknown content property type '{contentProp.PropertyType.FullName}'");
                }
            }
            else
            {
                Log.Warning("Cannot add {view} under {parent} as parent doesn't contain content property", view, parent);
            }
        }

        void ApplyProperty(object parent, XamlProperty xamlProperty)
        {
            if (parent == null)
            {
                Log.Warning("Parent is null");
                return;
            }
            if (xamlProperty == null)
            {
                Log.Warning("Property is null");
                return;
            }

            if (string.IsNullOrWhiteSpace(xamlProperty.Name))
            {
                Log.Verbose("Unnamed property is probably a content value");

                if (xamlProperty is XamlStringProperty)
                {
                    Log.Debug("Setting string content property");

                    var contentProp = ReflectionHelpers.GetContentProperty(parent.GetType());
                    if (contentProp == null)
                    {
                        Log.Warning($"No content property for {parent.GetType().FullName}");
                    }
                    else if (contentProp.PropertyType != typeof(string))
                    {
                        Log.Warning($"Content property should be string, it is {contentProp.PropertyType.FullName}");
                    }
                    else
                    {
                        Log.Debug($"Setting content property value {xamlProperty.GetString()}");
                        contentProp.SetValue(parent, xamlProperty.GetString());
                    }
                }
                else if (xamlProperty is XamlNodesProperty)
                {
                    Log.Debug("Setting nodes content property");

                    var nodes = xamlProperty.GetNodes();
                    if (!nodes.Any())
                    {
                        Log.Warning("No nodes found under {property}", xamlProperty);
                    }
                    else if (nodes.Any())
                    {
                        foreach (var node in nodes)
                        {
                            var view = CreateNode(node) as View;
                            if (view != null)
                                AddToParent(parent, view);
                        }
                    }
                }
                else
                {
                    Log.Warning($"Unsupported property type {xamlProperty.GetType().FullName}");
                }
            }
            else if (xamlProperty.Name.Contains("."))
            {
                Log.Debug("Setting attached {property}", xamlProperty);
                HandleAttachedProperty(parent, xamlProperty.Name, xamlProperty.GetString());
            }
            else
            {
                Log.Debug("Setting regular {property}", xamlProperty);
                HandleProperty(parent, xamlProperty);
            }
        }

        void HandleProperty(object parent, XamlProperty xamlProperty)
        {
            if (parent == null)
            {
                Log.Warning("Parent shouldn't be bull");
                return;
            }
            if (xamlProperty == null)
            {
                Log.Warning("Property shouldn't be null");
                return;
            }

            var prop = parent.GetType().GetRuntimeProperty(xamlProperty.Name);
            if (prop == null)
            {
                Log.Warning($"No property {xamlProperty.Name} found under {parent.GetType().FullName}");
                return;
            }

            Log.Verbose("Found property {prop}", prop);

            if (xamlProperty is XamlStringProperty)
            {
                Log.Debug("Setting string attribute");
                Apply(parent, prop, xamlProperty.GetString());
            }
            else if (xamlProperty is XamlNodesProperty)
            {
                Log.Debug("Setting object attribute");

                var values = xamlProperty.GetNodes()
                                         .Select(node => CreateNode(node))
                                         .Where(v => v != null)
                                         .ToList();

                if (!values.Any())
                {
                    Log.Debug($"No values found for property {prop.Name}");
                    return;
                }

                if (prop.PropertyType == typeof(ResourceDictionary))
                {
                    var resDict = values.OfType<ResourceDictionary>().FirstOrDefault();
                    if (resDict != null)
                    {
                        Log.Debug($"Setting resource dictionary to parent {parent.GetType().FullName}");
                        prop.SetValue(parent, resDict);
                    }
                }
                else if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(prop.PropertyType.GetTypeInfo()))
                {
                    Log.Debug("Target property is a an IEnumerable");

                    var propValue = prop.GetValue(parent);
                    if (propValue == null)
                    {
                        Log.Debug($"Property {prop.Name} is null, try creating it");

                        try
                        {
                            prop.SetValue(parent, propValue = Activator.CreateInstance(prop.PropertyType));
                        }
                        catch
                        {
                            Log.Warning($"Unable to set property {prop.Name} with new instance of {prop.PropertyType.FullName}");
                        }
                    }

                    if (propValue != null)
                    {
                        var addMethod = prop.PropertyType
                                           .GetRuntimeMethods()
                                           .FirstOrDefault(m => m.Name == "Add");

                        if (addMethod == null)
                        {
                            Log.Warning($"Cannot find Add method for {prop.PropertyType.FullName}");
                            return;
                        }

                        foreach (var value in values)
                        {
                            try
                            {
                                addMethod.Invoke(propValue, new[] { value });
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, $"Unable to add {{value}} to property {prop.Name}", value);
                            }
                        }
                    }
                }
                else
                {
                    var value = values.First();
                    Log.Debug($"Setting single {{value}} for property {prop.Name}", value);

                    if (prop.PropertyType.GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo()))
                    {
                        Log.Verbose($"Setting {prop.Name}");
                        prop.SetValue(parent, values[0]);
                    }
                    else if (value?.GetType()?.Name?.StartsWith("OnPlatform") == true)
                    {
                        Log.Verbose("Setting OnPlatform property");
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
                        else
                        {
                            Log.Warning($"Platform {Platform} is not supported");
                        }
                    }
                    else
                    {
                        Log.Warning($"Value type {value.GetType().FullName} is not compatible with {prop.PropertyType.FullName}");
                    }
                }
            }
        }

        object CreateNode(XamlNode node, object parent = null)
        {
            var element = CreateNodeInternal(node, parent);
            if (element != null)
                nodeToElementMapper.Add(node, element);

            return element;
        }

        object CreateNodeInternal(XamlNode node, object parent = null)
        {
            if (node == null)
            {
                Log.Warning("Cannot create node from null value");
                return null;
            }

            if (node.Name == "DataTemplate")
            {
                Log.Debug("Creating DataTemplate");

                var dt = new DataTemplate(() =>
                {
                    if (node.Children.Any())
                        return CreateNode(node.Children.FirstOrDefault());
                    return null;
                });

                return dt;
            }

            var type = ReflectionHelpers.GetType(node.Name);
            if (type == null)
            {
                Log.Information($"{node.Name} not found in View types");

                type = ReflectionHelpers.GetAllType(node.Name);
                if (type == null)
                    Log.Information($"{node.Name} not found in all types");
            }

            if (type != null)
            {
                Log.Debug($"Found type for {node.Name}");

                object element = null;

                // special case for certain types
                var typeConverter = ReflectionHelpers.GetTypeConverter(type);
                if (typeConverter != null && node.Properties.Any(p => p.IsContent))
                {
                    Log.Debug($"Node {{node}} is constructed via type converter {typeConverter.GetType().FullName}", node);
                    element = typeConverter.ConvertFromInvariantString(node.Properties.First(p => p.IsContent).GetString());
                }

                if (type == typeof(Style))
                {
                    var targetTypeProp = node.Properties
                                         .OfType<XamlStringProperty>()
                                         .FirstOrDefault(p => p.Name == nameof(Style.TargetType));
                    if (targetTypeProp == null)
                    {
                        Log.Error($"Invalid style, missing TargetType property");
                        return null;
                    }

                    var targetType = ReflectionHelpers.GetType(targetTypeProp.GetString());
                    if (targetType == null)
                    {
                        Log.Error($"Invalid target type for style: '{targetTypeProp.GetString()}'");
                        return null;
                    }

                    Log.Debug($"Creating style for target type {targetType.FullName}");
                    element = Activator.CreateInstance(type, new[] { targetType });
                }

                if (element == null)
                {
                    element = Activator.CreateInstance(type);
                }

                if (element is Setter)
                {
                    var propertyProp = node.Properties.FirstOrDefault(p => p.Name == nameof(Setter.Property));
                    BindableProperty setterProp = null;
                    if (propertyProp != null)
                    {
                        if (parent is Style)
                        {
                            var targetType = ((Style)parent).TargetType;
                            setterProp = targetType.GetRuntimeFields()
                                                   .Select(field => field.GetValue(null))
                                                   .OfType<BindableProperty>()
                                                   .FirstOrDefault(bp => bp.PropertyName == propertyProp.GetString());

                            if (setterProp != null)
                                ((Setter)element).Property = setterProp;
                        }
                    }

                    var valueProp = node.Properties
                                        .FirstOrDefault(p => p.Name == nameof(Setter.Value));
                    if (valueProp != null && setterProp != null)
                    {
                        typeConverter = setterProp.DeclaringType
                                                  .GetRuntimeProperty(setterProp.PropertyName)
                                                  .IfNotNull(p => ReflectionHelpers.GetTypeConverterForProperty(p));

                        var value = valueProp.GetString();
                        object converted;
                        if (typeConverter != null)
                        {
                            converted = typeConverter.ConvertFromInvariantString(value);
                            ((Setter)element).Value = converted;
                        }
                        else if (Parse(setterProp.ReturnType, value, out converted))
                        {
                            ((Setter)element).Value = converted;
                        }
                    }
                }
                else
                {
                    foreach (var prop in node.Properties)
                        ApplyProperty(element, prop);
                }

                foreach (var child in node.Children)
                {
                    if (element is ResourceDictionary)
                    {
                        var key = child.Properties
                                       .FirstOrDefault(p => p.Name == "Key")
                                       .IfNotNull(p =>
                                       {
                                           if (p is XamlStringProperty)
                                               return p.GetString();
                                           return null;
                                       });

                        var value = CreateNode(child);
                        if (key == null)
                        {
                            Log.Error("Adding static resource without a key!");
                        }
                        else if (value == null)
                        {
                            Log.Error("Adding static resource with null value!");
                        }
                        else
                        {
                            Log.Debug($"Adding static resource {key} to resource dictionary");
                            element.Cast<ResourceDictionary>()
                                   .Add(key, value);
                        }
                    }
                    else
                    {
                        AddChild(element, child);
                    }
                }

                return element;
            }

            if (node.Name == "OnPlatform")
            {
                Log.Debug("Creating OnPlatform Node");

                // find generic OnPlatform type
                type = ReflectionHelpers.AllTypes.FirstOrDefault(t => t.Key.Contains("OnPlatform")).Value;

                var typeArgumentProperty = node.Properties.FirstOrDefault(p => p.Name == "TypeArguments");
                if (typeArgumentProperty == null)
                {
                    Log.Error("Missing explicit type argument in OnPlatform node");
                }
                else if (typeArgumentProperty != null)
                {
                    var typeArgumentTypeName = typeArgumentProperty.GetString();
                    var typeArgument = ReflectionHelpers.GetType(typeArgumentTypeName) ??
                                       ReflectionHelpers.GetAllType(typeArgumentTypeName);

                    if (typeArgument == null)
                    {
                        Log.Error($"Unable to find type for {typeArgumentTypeName}");
                    }
                    else
                    {
                        // create concrete type in the form OnPlatform<Type>
                        type = type.MakeGenericType(new[] { typeArgument });
                        var element = Activator.CreateInstance(type);

                        // apply all child properties
                        foreach (var prop in node.Properties)
                        {
                            ApplyProperty(element, prop);
                        }

                        return element;
                    }
                }
            }

            Log.Debug($"Unable to create from {node.Name}");
            return null;
        }

        public void HandleAttachedProperty(object o, string property, string value)
        {
            if (o == null)
            {
                Log.Warning("Target object is null");
                return;
            }
            if (string.IsNullOrWhiteSpace(property))
            {
                Log.Warning("Invalid property name.");
                return;
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                Log.Warning("Ignoring null value for property");
                return;
            }

            var parts = property.Split(new[] { '.' });

            if (parts.Length != 2)
            {
                Log.Error($"Invalid attached property '{property}'");
                return;
            }

            Type owner = ReflectionHelpers.GetType(parts[0]);
            if (owner == null)
            {
                Log.Error($"Owner type for attached property '{parts[0]}' not found");
                return;
            }

            var methods = owner.GetTypeInfo().DeclaredMethods;
            var method = methods.FirstOrDefault(x => x.Name == $"Set{parts[1]}");
            if (method == null)
            {
                Log.Error($"Cannot find setter method for attached property '{parts[1]}' on type '{parts[0]}'");
                return;
            }

            var parameters = method.GetParameters();
            if (parameters.Length != 2)
            {
                Log.Error($"Invalid number of arguments for method '{method.Name}': got {parameters.Length}, expected 2");
                return;
            }

            if (!parameters[0].ParameterType.GetTypeInfo().IsAssignableFrom(o.GetType().GetTypeInfo()))
            {
                Log.Error($"Setter method expected '{parameters[0].ParameterType.FullName}', provided object was of type '{o.GetType().FullName}'");
                return;
            }

            object parsed;
            if (!Parse(parameters[1].ParameterType, value, out parsed))
            {
                Log.Error($"Cannot parse value '{value}' for attached property {property}");
                return;
            }

            if (parsed == null)
            {
                Log.Error($"Ignoring null values for attached property {property}");
                return;
            }

            if (!parameters[1].ParameterType.GetTypeInfo().IsAssignableFrom(parsed.GetType().GetTypeInfo()))
            {
                Log.Error($"Invalid parsed value type. Expected '{parameters[1].ParameterType.FullName}', got '{parsed.GetType().FullName}'");
                return;
            }

            try
            {
                method.Invoke(null, new object[] { o, parsed });
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}
