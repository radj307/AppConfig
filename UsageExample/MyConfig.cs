using AppConfig;
using Newtonsoft.Json;
using System.ComponentModel;

namespace UsageExample
{
    public class ConfigSection
    {
        [JsonIgnore]
        public int IgnoredSubField = 99;
        public int SubField1 = 5;
        public string SubProperty1 { get; set; } = "Hello World!";
        public int SubProperty2 { get; set; } = 1000;
    }
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
        public ConfigSection Section { get; set; } = new();
    }
}
