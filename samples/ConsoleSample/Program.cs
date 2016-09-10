// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using Sancho.XAMLParser;
using Serilog;

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
            
            var parser = new Parser();
            var root = parser.Parse(@"<Button Text=""Hello"" NotAProp=""what?"" />");
            Log.Debug("Root {@Root}", root);
        }
    }
}
