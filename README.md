# AppReconfig
Lightweight C# application configuration using JSON format.

For the best results, this should be used with [Fody](https://github.com/Fody/Fody) & its [PropertyChanged](https://github.com/Fody/PropertyChanged) addon so you don't have to write your own `PropertyChanged` notifiers.  

## Getting Started

Check out the [example projects](Examples/UsageExample) for full examples.

To use it, create a class to store your settings and inherit from one of the `Configuration` abstract classes provided by AppConfig:

```csharp
using AppConfig;

namespace UsageExample
{
    public class MyConfig : ConfigurationFileWithAutosave
    {
        public MyConfig(string location) : base(location) { }

        public static MyConfig Instance => (MyConfig)DefaultInstance;

        public string Text { get; set; } = string.Empty;
        public bool BoxIsChecked { get; set; } = false;
    }
}
```

You can even use it in WPF:  

```xaml
<StackPanel Grid.Row="1" Orientation="Horizontal">
    <!--  This checkbox uses the ViewModel object as the data binding source  -->
    <CheckBox
        Margin="10,5"
        VerticalAlignment="Center"
        Content="Box"
        IsChecked="{Binding BoxIsChecked, Source={StaticResource VM}, UpdateSourceTrigger=PropertyChanged}" />

    <!--  This textbox uses the static Instance property as the data binding source  -->
    <TextBox
        Margin="10,5"
        HorizontalAlignment="Stretch"
        VerticalAlignment="Stretch"
        Text="{Binding Text, Source={x:Static local:MyConfig.Instance}, UpdateSourceTrigger=LostFocus}" />
</StackPanel>
```
