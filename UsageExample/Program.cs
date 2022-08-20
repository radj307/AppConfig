using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UsageExample
{
    internal static class Program
    {
        /// <summary>
        /// This field has ownership of the <see cref="AppConfig.Configuration.Default"/> instance.<br/>
        /// </summary>
        private static readonly MyConfig _config = new("ExampleConfig.json");

        [STAThread]
        public static void Main(string[] args)
        {
            // Load the previous config file
            _config.Load();

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
