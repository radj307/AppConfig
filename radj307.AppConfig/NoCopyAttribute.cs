namespace AppConfig
{
    /// <summary>
    /// Prevents the <see cref="ConfigBase.DeepCopy(ConfigBase, ConfigBase)"/> method from processing the attached member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
    public class NoCopyAttribute : Attribute { }
}