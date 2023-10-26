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

Check out the full [`UsageExample`](UsageExample)

```csharp
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
```
