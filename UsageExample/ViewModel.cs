namespace UsageExample
{
    public class ViewModel
    {
        /// <summary>
        /// This property accesses the <see cref="AppConfig.Configuration.Default"/> <see langword="static"/> instance, which is automatically set to the first instantiated subclass.
        /// </summary>
        public MyConfig Config => (MyConfig.Default as MyConfig)!;
    }
}
