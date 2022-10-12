# AppConfig
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
using System.ComponentModel;

namespace UsageExample
{
    // Create a class that derives from AppConfig.Configuration (custom file loader implementation) OR AppConfig.ConfigurationFile (JSON)
    public class MyConfig : AppConfig.ConfigurationFile, INotifyPropertyChanged
    {
        public MyConfig(string? locationOverride = null) : base(locationOverride ?? "ExampleConfig.json")
        {
            // This will automatically save the config whenever a property changes:
            this.PropertyChanged += (s, e) => this.Save();
        }
        public MyConfig() : this(null) { }

        public string Text { get; set; } = string.Empty;
        public bool BoxIsChecked { get; set; } = false;
    }
}
```
