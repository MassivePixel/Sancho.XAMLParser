// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using Xamarin.Forms;

namespace Sancho.DOM.XamarinForms
{
    public class XamlNodeHelpers
    {
        public static bool IsContentNode(string nodeName, string propName)
        {
            if (string.IsNullOrEmpty(nodeName) ||
                string.IsNullOrEmpty(propName))
                return false;

            var lastDot = propName.LastIndexOf(".");
            if (lastDot >= 0)
            {
                propName = propName.Substring(lastDot + 1);
            }

            var type = ReflectionHelpers.GetType(nodeName)
                                        ?? (nodeName == "ViewCell"
                                            ? typeof(ViewCell)
                                            : null);
            if (type != null)
            {
                var contentPropName = ReflectionHelpers.GetContentProperty(type)?.Name;
                return !string.IsNullOrWhiteSpace(contentPropName) &&
                              contentPropName == propName;
            }

            return false;
        }
    }
}
