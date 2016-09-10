using Serilog;
using SimpleXamlParser;

namespace ConsoleSample
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile("logs\\myapp-{Date}.txt")
                .CreateLogger();
            
            var parser = new Parser(null);
            var root = parser.Parse(@"<Button Text=""Hello"" NotAProp=""what?"" />");
            Log.Debug("Root {@Root}", root);
        }
    }
}
