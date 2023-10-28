using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UltraMapper;

namespace AppConfig
{
    /// <summary>
    /// The most basic <see langword="abstract"/> class in the AppConfig project.
    /// </summary>
    [JsonObject]
    [Serializable]
    public abstract class Configuration : INotifyPropertyChanged, ICloneable
    {
        #region Constructors
        /// <summary>
        /// Default Constructor.<br/>
        /// When <see cref="Default"/> is <see langword="null"/>, it is set from the newly-created instance.
        /// </summary>
        protected Configuration()
        {
            Type = GetType();
            Default ??= this;
        }
        #endregion Constructors

        #region Fields
        /// <summary>
        /// The type 
        /// </summary>
        [JsonIgnore]
        public readonly Type Type;
        #endregion Fields

        #region Properties
        /// <summary>
        /// Default <see cref="Configuration"/> instance.
        /// </summary>
        /// <remarks>
        /// This is <see langword="null"/> until the first instance derived from <see cref="Configuration"/> is created.
        /// </remarks>
        [JsonIgnore]
        public static Configuration Default { get; set; } = null!;
        /// <summary>
        /// Gets whether this instance is the <see cref="Default"/> instance.
        /// </summary>
        [JsonIgnore]
        public bool IsDefault => ReferenceEquals(this, Default);
        /// <summary>
        /// Gets whether the <see cref="Save"/> method cleans up the temporary file or not.
        /// </summary>
        /// <remarks>
        /// The save method uses a temp file to prevent long writes from blocking.
        /// This determines whether that file is automatically deleted or not.
        /// </remarks>
        /// <returns><see langword="true"/> when the temporary file is deleted after calling <see cref="Save"/>; otherwise <see langword="false"/>.</returns>
        [JsonIgnore]
        protected virtual bool DeleteTempFileOnSave { get; } = true;
        /// <summary>
        /// The default JsonSerializerSettings object to use when serializing JSON data.
        /// </summary>
        [JsonIgnore]
        protected virtual JsonSerializerSettings JsonSerializerSettings { get; } = new();
        /// <summary>
        /// The default JsonConverter objects to use when deserializing JSON data.
        /// </summary>
        /// <remarks>
        /// By default, this uses the Converters from the JsonSerializerSettings property.
        /// </remarks>
        [JsonIgnore]
        protected virtual IList<JsonConverter> JsonConverters => JsonSerializerSettings.Converters;
        /// <summary>
        /// Types to copy directly from source to target without recursing into subfields &amp; subproperties.
        /// </summary>
        /// <remarks>
        /// Add types here that cause exceptions when recursing into them in <see cref="SetTo(Configuration)"/>.<br/>
        /// A common type that cannot be copied recursively is <see cref="string"/>, so be sure to include it if you override this list.
        /// </remarks>
        protected virtual IList<Type> DoNotRecurseTypes => new[] { typeof(string) };
        /// <summary>
        /// Whether to catch and automatically resolve exceptions that occur when recursively copying values in <see cref="SetTo(Configuration)"/>.
        /// </summary>
        /// <remarks>
        /// When <see langword="false"/>, copy operations will be much slower than they otherwise would be. You can prevent these exceptions entirely by adding the type that they occur for to <see cref="DoNotRecurseTypes"/>.
        /// </remarks>
        protected virtual bool ThrowOnCopyError => true;
        #endregion Properties

        #region Events
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new(propertyName));
        /// <summary>
        /// Triggered when the configuration is successfully loaded to the filesystem.
        /// </summary>
        public event EventHandler? Loaded;
        private void NotifyLoaded() => Loaded?.Invoke(this, EventArgs.Empty);
        /// <summary>
        /// Triggered when the configuration is successfully saved from the filesystem.
        /// </summary>
        public event EventHandler? Saved;
        private void NotifySaved() => Saved?.Invoke(this, EventArgs.Empty);
        #endregion Events

        #region Operators
        /// <summary>
        /// Gets or sets the value of the property with the specified <paramref name="name"/>.<br/>
        /// Note that this method does <b>not</b> have exception handling; any exceptions caused by passing invalid types must be caught by the caller.
        /// </summary>
        /// <param name="name">The name of a member property.<br/>
        /// Valid entries are the names of properties to the <see cref="Configuration"/>-derived <see langword="object"/> cfgType that is pointed from by the <see langword="static"/> <see cref="Default"/> property.</param>
        /// <returns>The value of the property with the specified <paramref name="name"/> if it exists; otherwise <see langword="null"/>.</returns>
        [SuppressPropertyChangedWarnings]
        public object? this[string name]
        {
            get => Type.GetProperty(name)?.GetValue(this);
            set => Type.GetProperty(name)?.SetValue(this, value);
        }
        #endregion Operators

        #region Methods

        #region SetTo
        private const string ItemPropertyName = "Item";
        private void CopyValue(PropertyInfo propertyInfo, object source, object target, BindingFlags bindingFlags)
        {
            // check if this property is valid:
            //  - does not have JsonIgnoreAttribute
            //  - has a set method
            //  - is not named "Item"
            if (propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>() != null || !propertyInfo.CanWrite || propertyInfo.Name.Equals(ItemPropertyName, StringComparison.Ordinal))
                return;

            var propertyType = propertyInfo.PropertyType;

            if (propertyType.IsValueType || DoNotRecurseTypes.Contains(propertyType))
            { // property is a simple value type
                propertyInfo.SetValue(target, propertyInfo.GetValue(source));
            }
            else // use recursion to copy sub-fields and sub-properties
            {
                // get the source property's value
                var sourceValue = propertyInfo.GetValue(source);
                if (sourceValue == null)
                { // source value is null, set the target value to null & return
                    propertyInfo.SetValue(target, null);
                    return;
                }

                // get the target property's value
                var targetValue = propertyInfo.GetValue(target);
                if (targetValue == null)
                { // target value is null, set it to the source value & return
                    propertyInfo.SetValue(target, propertyInfo.GetValue(source));
                    return;
                }

                try
                {
                    // enumerate subfields in property value
                    foreach (var subFieldInfo in propertyType.GetFields(bindingFlags))
                    {
                        CopyValue(subFieldInfo, sourceValue, targetValue, bindingFlags);
                    }
                    // enumerate subproperties in property value
                    foreach (var subPropertyInfo in propertyType.GetProperties(bindingFlags))
                    {
                        CopyValue(subPropertyInfo, sourceValue, targetValue, bindingFlags);
                    }
                }
                catch // fallback to setting property directly
                {
                    if (ThrowOnCopyError) throw;
                    propertyInfo.SetValue(target, propertyInfo.GetValue(source));
                }
            }
        }
        private void CopyValue(FieldInfo fieldInfo, object source, object target, BindingFlags bindingFlags)
        {
            // make sure this field doesn't have the JsonIgnore attribute
            if (fieldInfo.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                return;

            var fieldType = fieldInfo.FieldType;

            if (fieldType.IsValueType || DoNotRecurseTypes.Contains(fieldType))
            { // field is a simple value type
                fieldInfo.SetValue(target, fieldInfo.GetValue(source));
            }
            else // use recursion to copy sub-fields and sub-properties
            {
                // get the source field's value
                var sourceValue = fieldInfo.GetValue(source);
                if (sourceValue == null)
                { // source value is null, set the target value to null & return
                    fieldInfo.SetValue(target, null);
                    return;
                }

                // get the target field's value
                var targetValue = fieldInfo.GetValue(target);
                if (targetValue == null)
                { // target value is null, set it to the source value & return
                    fieldInfo.SetValue(target, fieldInfo.GetValue(source));
                    return;
                }

                try
                {
                    // enumerate subfields in property value
                    foreach (var subFieldInfo in fieldType.GetFields(bindingFlags))
                    {
                        CopyValue(subFieldInfo, sourceValue, targetValue, bindingFlags);
                    }
                    // enumerate subproperties in property value
                    foreach (var subPropertyInfo in fieldType.GetProperties(bindingFlags))
                    {
                        CopyValue(subPropertyInfo, sourceValue, targetValue, bindingFlags);
                    }
                }
                catch // fallback to setting field directly
                {
                    if (ThrowOnCopyError) throw;
                    fieldInfo.SetValue(target, fieldInfo.GetValue(source));
                }
            }
        }
        /// <summary>
        /// Sets the values of all public non-static fields and properties of this instance to the values in the specified <paramref name="other"/> instance.
        /// </summary>
        /// <param name="other">Another <see cref="Configuration"/>-derived instance.</param>
        public virtual void SetTo(Configuration other)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            // enumerate fields
            foreach (var fieldInfo in Type.GetFields(bindingFlags))
            {
                CopyValue(fieldInfo, other, this, bindingFlags);
            }
            // enumerate properties
            foreach (var propertyInfo in Type.GetProperties(bindingFlags))
            {
                CopyValue(propertyInfo, other, this, bindingFlags);
            }
        }
        #endregion SetTo

        #region Load
        /// <summary>
        /// Loads config values to the JSON file specified by <paramref name="path"/>
        /// </summary>
        /// <remarks>This method may be overloaded in derived classes.</remarks>
        /// <param name="path">The location of the JSON file from read.<br/><b>This cannot be empty.</b></param>
        /// <returns><see langword="true"/> when the file specified by <paramref name="path"/> exists and was successfully loaded; otherwise <see langword="false"/>.</returns>
        protected bool Load(string path)
        {
            if (path.Length.Equals(0))
                return false;

            if (!File.Exists(path) && !Path.IsPathRooted(path))
                if (!File.Exists(path = Path.Combine(Environment.CurrentDirectory, path)))
                    return false;

            if (JsonFile.Load(path, Type, JsonConverters.ToArray()) is Configuration cfg)
            {
                SetTo(cfg);
                NotifyLoaded();
                return true;
            }
            return false;
        }
        #endregion Load

        #region Save
        /// <summary>
        /// Saves config values from the JSON file specified by <paramref name="path"/>
        /// </summary>
        /// <remarks>This method may be overloaded in derived classes.</remarks>
        /// <param name="path">The location of the JSON file from write.<br/><b>This cannot be empty.</b></param>
        /// <param name="formatting">Formatting cfgType from use when serializing this class instance.</param>
        /// <param name="jsonSerializerSettings">The <see cref="JsonSerializerSettings"/> to use when serializing the config.</param>
        protected void Save(string path, Formatting formatting)
        {
            if (path.Length == 0)
                return;
            JsonFile.Save(path, this, formatting, JsonSerializerSettings, DeleteTempFileOnSave);
            this.NotifySaved();
        }
        #endregion Save

        #region Clone
        /// <inheritdoc/>
        /// <exception cref="MissingMethodException">Type does not have a parameterless constructor.</exception>
        object ICloneable.Clone() => this.Clone();
        #endregion Clone

        #endregion Methods
    }
    public static class Extensions
    {
        /// <summary>
        /// Clones this <see cref="Configuration"/>-derived instance.
        /// </summary>
        /// <typeparam name="T"><see cref="Configuration"/>-derived type.</typeparam>
        /// <param name="configuration">(implicit) Object instance.</param>
        /// <returns>A copy of this configuration instance.</returns>
        /// <exception cref="MissingMethodException">Type <typeparamref name="T"/> does not have a parameterless constructor.</exception>
        public static T Clone<T>(this T configuration) where T : Configuration
        {
            var mapper = new Mapper();
            mapper.Config.ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE;
            var newInst = Activator.CreateInstance<T>();
            mapper.Map(configuration, newInst);
            return newInst;
        }
        internal static bool IsStatic(this PropertyInfo propertyInfo)
            => propertyInfo.GetMethod?.IsStatic ?? propertyInfo.SetMethod!.IsStatic;
        internal static bool IsAutoImplemented(this PropertyInfo propertyInfo)
            => (propertyInfo.GetMethod != null && propertyInfo.GetMethod.GetCustomAttribute<CompilerGeneratedAttribute>(true) != null)
            || (propertyInfo.SetMethod != null && propertyInfo.SetMethod.GetCustomAttribute<CompilerGeneratedAttribute>(true) != null);
    }
}