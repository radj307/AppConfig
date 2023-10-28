# AppReconfig
Lightweight C# application configuration using JSON format.

For the best results, this should be used with [Fody](https://github.com/Fody/Fody) & its [PropertyChanged](https://github.com/Fody/PropertyChanged) addon so you don't have to write your own `PropertyChanged` notifier.  

To quickly get started with Fody if you haven't used it before, create a `FodyWeavers.xml` file in your **project directory** with the content:
```xml
<Weavers>
  <PropertyChanged/>
</Weavers>
```

## Example

Check out the full [`UsageExample`](UsageExample) project.

```csharp
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
```
