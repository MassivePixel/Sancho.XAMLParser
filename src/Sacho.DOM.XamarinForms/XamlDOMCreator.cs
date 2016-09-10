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
    public class XamlDOMCreator : IXamlDOM
    {
        public object Root { get; private set; }

        public TargetPlatform Platform { get; set; }

        public XamlDOMCreator()
        {
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

            AddChild(null, node);
        }

        public void AddChild(object parent, XamlNode node)
        {
            var element = CreateNode(node);

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
        }

        public void AddToParent(object parent, View view)
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

        public void ApplyProperty(object parent, XamlProperty xamlProperty)
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

        public void HandleProperty(object parent, XamlProperty xamlProperty)
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
                AttributeHelper.Apply(parent, prop, xamlProperty.GetString());
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

                if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(prop.PropertyType.GetTypeInfo()))
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

        public object CreateNode(XamlNode node)
        {
            if (node == null)
            {
                Log.Warning("Cannot create node from null value");
                return null;
            }

            if (node.Name == "DataTemplate")
            {
                Log.Debug("Creating DataTemplate");

                return new DataTemplate(() =>
                {
                    if (node.Children.Any())
                        return CreateNode(node.Children.FirstOrDefault());
                    return null;
                });
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
                var element = Activator.CreateInstance(type);

                foreach (var child in node.Children)
                    AddChild(element, child);

                foreach (var prop in node.Properties)
                    ApplyProperty(element, prop);

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
            if (!AttributeHelper.Parse(parameters[1].ParameterType, value, out parsed))
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
