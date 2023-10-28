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
    public class MyConfig : ConfigurationFileWithAutosave, INotifyPropertyChanged
    {
        public MyConfig(string location) : base(location) { }

        public static MyConfig Instance => (MyConfig)DefaultInstance;

        public string Text { get; set; } = string.Empty;
        public bool BoxIsChecked { get; set; } = false;
        public ConfigSection Section { get; set; } = new();
    }
}
