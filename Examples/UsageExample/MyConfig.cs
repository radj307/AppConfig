using AppConfig;

namespace UsageExample
{
    public class MyConfig : ConfigurationFileWithAutosave
    {
        public MyConfig(string location) : base(location) { }

        public static MyConfig Instance => (MyConfig)DefaultInstance;

        public string Text { get; set; } = string.Empty;
        public bool BoxIsChecked { get; set; } = false;
        public bool EnableAnonymousPropertyAccessExample { get; set; } = false;
    }
}
