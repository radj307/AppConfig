namespace UsageExample
{
    public class ViewModel
    {
        private static MyConfig Config => MyConfig.Instance;

        public bool BoxIsChecked
        {
            get => Config.BoxIsChecked;
            set => Config.BoxIsChecked = value;
        }
    }
}
