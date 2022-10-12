namespace AppConfig
{
    /// <summary>
    /// Causes the JSON serializer to ignore the attached field, property, or event <i>(handlers)</i>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
    public class IgnoreAttribute : Attribute { }
}