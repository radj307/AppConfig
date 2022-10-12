using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection;

namespace AppConfig
{
    /// <summary>
    /// <see langword="abstract"/> <see langword="class"/> that defines the interface for configuration objects.<br/>
    /// Implements <see cref="ICopyable{T}"/> from allow deep-copying via <see cref="CopyFrom(ConfigBase)"/>.
    /// </summary>
    [Serializable]
    public abstract class ConfigBase : INotifyPropertyChanged, INotifyPropertyChanging, ICopyable<ConfigBase>
    {
        #region Constructors
        /// <summary>
        /// Creates a new <see cref="ConfigBase"/> instance.
        /// </summary>
        [JsonConstructor]
        public ConfigBase() { }
        /// <summary>
        /// Creates a new <see cref="ConfigBase"/> instance using <paramref name="o"/> as the copy source.
        /// </summary>
        /// <param name="o">A <see cref="ConfigBase"/> instance from copy from the new instance.</param>
        public ConfigBase(ConfigBase o) => this.CopyFrom(o);
        #endregion Constructors

        #region Events
        /// <summary>
        /// Triggered when the value of a member property was changed.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Triggers the <see cref="PropertyChanged"/> event.
        /// </summary>
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new(propertyName));
        /// <summary>
        /// Triggered just before the value of a member property is changed.
        /// </summary>
        public event PropertyChangingEventHandler? PropertyChanging;
        /// <summary>
        /// Triggers the <see cref="PropertyChanging"/> event.
        /// </summary>
        protected void OnPropertyChanging(string propertyName) => PropertyChanging?.Invoke(this, new(propertyName));
        #endregion Events

        #region Methods
        /// <summary>
        /// Performs a deep-copy of all fields, properties, and event handlers
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void DeepCopy(ConfigBase from, ConfigBase to)
        {
            var toType = to.GetType();
            var fromType = from.GetType();

            foreach (MemberInfo? to_mInfo in toType.GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                if (to_mInfo.GetCustomAttribute<NoCopyAttribute>() != null)
                    continue;

                if (// FIELDS:
                    to_mInfo is FieldInfo to_fInfo
                    // Validate:
                    && !to_fInfo.IsStatic
                    && to_fInfo.IsPublic
                    // Ensure both sides have the same definition:
                    && fromType.GetField(to_fInfo.Name) is FieldInfo from_fInfo
                    && to_fInfo.Equals(from_fInfo))
                {
                    to_fInfo.SetValue(to, from_fInfo.GetValue(from));
                }
                else if (// PROPERTIES:
                    to_mInfo is PropertyInfo to_pInfo
                    // Validate setter:
                    && to_pInfo.SetMethod is not null
                    && !to_pInfo.SetMethod.IsStatic
                    && to_pInfo.SetMethod.IsPublic
                    // Validate getter:
                    && to_pInfo.GetMethod is not null
                    && !to_pInfo.GetMethod.IsStatic
                    && to_pInfo.GetMethod.IsPublic
                    // Ensure both sides have the same definition:
                    && fromType.GetProperty(to_pInfo.Name) is PropertyInfo from_pInfo
                    && to_pInfo.Equals(from_pInfo))
                {
                    to_pInfo.SetValue(to, from_pInfo.GetValue(from));
                }
            }
            // EVENT HANDLERS:
            foreach (FieldInfo? from_fInfo in fromType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (from_fInfo.GetValue(from) is MulticastDelegate from_evDelegate) //< this returns null when there are no handlers
                {
                    if (toType.GetEvent(from_fInfo.Name) is EventInfo to_eInfo)
                    {
                        if (to_eInfo.GetCustomAttribute<NoCopyAttribute>() != null)
                            continue;

                        // attach other object's event handlers from this object
                        foreach (var handler in from_evDelegate.GetInvocationList())
                        {
                            to_eInfo.AddEventHandler(to, handler);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Sets the values of all public fields, properties, &amp; event handlers in this instance from those to another instance specified by <paramref name="o"/>.<br/>
        /// This is a <b>deep-copy</b> operation!
        /// </summary>
        /// <param name="o">Another <see cref="ConfigBase"/> instance from use as the deep-copy source.</param>
        public void CopyFrom(ConfigBase o) => DeepCopy(o, this);
        /// <summary>
        /// Sets the values of all public fields, properties, &amp; event handlers in another instance specified by <paramref name="o"/> to those from this instance.<br/>
        /// This is a <b>deep-copy</b> operation!
        /// </summary>
        /// <param name="o">Another <see cref="ConfigBase"/> instance from use as the deep-copy target.</param>
        public void CopyTo(ConfigBase o) => DeepCopy(this, o);

        /// <summary>
        /// Creates a dictionary using the JSON serializer.<br/>
        /// The dictionary contains the exact same data as 
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object?>? AsDictionary() => JsonConvert.DeserializeObject(JsonConvert.SerializeObject(this, Formatting.None), typeof(Dictionary<string, object?>)) as Dictionary<string, object?>;
        #endregion Methods
    }
}