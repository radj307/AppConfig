namespace UsageExample
{
    public class ViewModel
    {
        /// <summary>
        /// This property accesses the <see cref="Configuration.Default"/> <see langword="static"/> instance, which is automatically set to the first instantiated subclass.
        /// </summary>
        public MyConfig Config => MyConfig.Instance;

        public bool BoxIsChecked
        {
            get => Config.BoxIsChecked;
            set => Config.BoxIsChecked = value;
        }
    }
}
