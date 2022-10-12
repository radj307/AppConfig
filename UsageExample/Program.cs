using System;
using System.Windows;

namespace UsageExample
{
    public class Conf : AppConfig.ConfigBase
    {
        public string Text { get; set; } = string.Empty;
        public bool BoxIsChecked { get; set; } = false;

        public string? StringProp { get; set; } = "Hello";
        public string? StringField = "World!";

        public bool BoolProp { get; set; }        
        public bool BoolField;

        public bool BoolAccessor
        {
            get => BoolField;
            set => BoolField = value;
        }

        public event EventHandler? SomethingHappened;
        public void NotifySomethingHappened() => SomethingHappened?.Invoke(this, EventArgs.Empty);

        public static void HandleHappening(object? sender, EventArgs e)
        {
            MessageBox.Show($"SomethingHappened\n\n" +
                $"sender: ({sender})\n" +
                $"args:   ({e})\n");
        }
    }

    public static class Program
    {
        /// <summary>
        /// This field has ownership of the <see cref="AppConfig.Configuration.Default"/> instance.<br/>
        /// </summary>
        private static readonly MyConfig _config = new("ExampleConfig.json");
        public static readonly AppConfig.ConfigManager<Conf> configManager = new("ExampleConfig2.json");

        [STAThread]
        public static void Main(string[] args)
        {
            // Load the previous config file
            //_config.Load();
            configManager.AutoSave = true;
            configManager.Load();

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
