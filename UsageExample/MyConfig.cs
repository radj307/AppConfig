using AppConfig;
using System.ComponentModel;

namespace UsageExample
{
    public class MyConfig : ConfigurationFile, INotifyPropertyChanged
    {
        public MyConfig(string? locationOverride = null) : base(locationOverride ?? "ExampleConfig.json")
        {
            this.PropertyChanged += this.MyConfig_PropertyChanged;
        }

        private void MyConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Save();
        }

        public MyConfig() : this(null) { }

        public static MyConfig Instance => (MyConfig)Default;

        public string Text { get; set; } = string.Empty;
        public bool BoxIsChecked { get; set; } = false;
    }
}
