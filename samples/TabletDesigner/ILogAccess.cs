using System;
using Sancho.DOM.XamarinForms;
using Sancho.XAMLParser;
using TabletDesigner.Helpers;
using Xamarin.Forms;

namespace TabletDesigner
{
    public interface ILogAccess
    {
        void Clear();
        string Log { get; }
    }
    
}
