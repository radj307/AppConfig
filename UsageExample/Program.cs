using AppConfig;
using System;
using System.Windows;

namespace UsageExample
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            MyConfig config = new("ExampleConfig.json");
            config.Load();

            var app = new Application();
            try
            {
                var window = new MainWindow();
                app.Run(window);
            }
            catch (Exception)
            {
#           if DEBUG
                throw;
#           else
                // Do something else
#           endif
            }
        }
    }
}
