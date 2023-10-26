using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters;
using UltraMapper;

namespace AppConfig
{
    /// <summary>
    /// The most basic <see langword="abstract"/> class in the AppConfig project.
    /// </summary>
    /// <remarks>
    /// Note that this class implements the <see cref="INotifyPropertyChanged"/> interface using Fody, as a result, all derived classes will automatically have event triggers injected into their property setters.
    /// </remarks>
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
        /// <summary>
        /// Sets the values of all public non-static fields and properties of this instance to the values in the specified <paramref name="other"/> instance.
        /// </summary>
        /// <param name="other">Another <see cref="Configuration"/>-derived instance.</param>
        public virtual void SetTo(Configuration other)
        {
            Type otherType = other.GetType();
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            foreach (var member in Type.GetMembers(bindingFlags))
            {
                switch (member.MemberType)
                {
                case MemberTypes.Field:
                    {
                        var fieldInfo = (FieldInfo)member;
                        if (fieldInfo.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

                        var otherFieldInfo = otherType.GetField(fieldInfo.Name, bindingFlags);
                        if (otherFieldInfo == null || otherFieldInfo.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

                        var otherValue = otherFieldInfo.GetValue(other);
                        fieldInfo.SetValue(this, otherValue);
                        break;
                    }
                case MemberTypes.Property:
                    {
                        var propertyInfo = (PropertyInfo)member;
                        if (propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>() != null || propertyInfo.Name.Equals("Item", StringComparison.Ordinal)) continue;

                        var otherPropertyInfo = otherType.GetProperty(propertyInfo.Name, bindingFlags);
                        if (otherPropertyInfo == null || otherPropertyInfo.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

                        var otherValue = otherPropertyInfo.GetValue(other);
                        propertyInfo.SetValue(this, otherValue);
                        break;
                    }
                }
            }
        }
        public void SetTo2(Configuration other)
        {
            var mapper = new Mapper();
            mapper.Map(other, this);
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