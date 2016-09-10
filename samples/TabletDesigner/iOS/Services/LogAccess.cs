using System.IO;
using Serilog;
using TabletDesigner.iOS.Services;

[assembly: Xamarin.Forms.Dependency(typeof(LogAccess))]

namespace TabletDesigner.iOS.Services
{
    public class LogAccess : ILogAccess
    {
        StringWriter messages;

        public string Log => messages.GetStringBuilder().ToString();

        public LogAccess()
        {
            messages = new StringWriter();

            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.TextWriter(messages, outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();

            Serilog.Log.Information("Started...");
        }

        public void Clear() => messages.GetStringBuilder().Clear();
    }
}
