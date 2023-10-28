using AnonymousPropertyAccessExample;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace UsageExample
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            // initialize the config with auto-saving enabled:
            MyConfig config = new("ExampleConfig.json")
            {
                EnableAutosave = true
            };
            // load the config file from disk if it exists
            config.Load();

            if (config.EnableAnonymousPropertyAccessExample)
            {
                var sb = new StringBuilder();
                sb.AppendLine("The following properties were accessed anonymously by one of this project's dependencies:");

                string[] propertyNames = new[] { nameof(MyConfig.Text), nameof(MyConfig.BoxIsChecked), nameof(config.EnableAnonymousPropertyAccessExample) };
                var propertyValues = Example.GetAndSetPropertiesAnonymously(propertyNames);

                for (int i = 0, i_max = propertyValues.Length; i < i_max; ++i)
                {
                    var valueAsString = propertyValues[i]?.ToString() ?? "(null)";
                    sb.AppendLine($"  \"{propertyNames[i]}\" = \"{valueAsString}\"");
                }

                MessageBox.Show(sb.ToString());
            }

            // open the config file in the default text editor:
            Process.Start(new ProcessStartInfo(config.Location) { UseShellExecute = true })?.Dispose();

            var app = new Application();
            var window = new MainWindow();
            return app.Run(window);
        }
    }
}
