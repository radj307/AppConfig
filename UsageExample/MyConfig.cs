using System.ComponentModel;

namespace UsageExample
{
    public class MyConfig : AppConfig.ConfigurationFile, INotifyPropertyChanged
    {
        public MyConfig(string? locationOverride = null) : base(locationOverride ?? "ExampleConfig.json")
        {
            this.PropertyChanged += (s, e) => this.Save();
        }
        public MyConfig() : this(null) { }

        public string Text { get; set; } = string.Empty;
        public bool BoxIsChecked { get; set; } = false;
    }
}
