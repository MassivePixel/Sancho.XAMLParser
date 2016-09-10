using Foundation;
using UIKit;

namespace TabletDesigner.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            LoadApplication(new App());

            Sancho.DOM.XamarinForms.ReflectionHelpers.PlatformServices = new PlatformServices();

            return base.FinishedLaunching(app, options);
        }
    }
}
