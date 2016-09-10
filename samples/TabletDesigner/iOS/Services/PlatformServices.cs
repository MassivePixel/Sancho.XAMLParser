using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sancho.XAMLParser.Interfaces;

namespace TabletDesigner.iOS
{
    class PlatformServices : IPlatformServices
    {
        public List<Assembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().ToList();
        }
    }
}
