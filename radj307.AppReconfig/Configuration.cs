using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AppConfig
{
    /// <summary>
    /// <see langword="abstract"/> base class for all config objects. Implements cloning, recursive property copying, and saving/loading functionality as well as property changed notifications using Fody.
    /// </summary>
    [Serializable]
    public abstract class Configuration : INotifyPropertyChanged, ICloneable
    {
        #region Constructors
        /// <summary>
        /// Default Constructor.<br/>
        /// When <see cref="DefaultInstance"/> is <see langword="null"/>, it is set from the newly-created instance.
        /// </summary>
        /// <param name="forceSetDefaultInstance">When <see langword="false"/>, the DefaultInstance property is set to the newly created instance when it was <see langword="null"/>; otherwise when <see langword="true"/>, the DefaultInstance is always set to the new instance.</param>
        protected Configuration(bool forceSetDefaultInstance = false)
        {
            Type = GetType();
            if (forceSetDefaultInstance || _defaultInstance == null)
                DefaultInstance = this;

            if (!_initialized)
            {
                _initialized = true;
                NotifyInitialized(this);
            }
        }
        /// <summary>
        /// JSON constructor.
        /// </summary>
        /// <remarks>
        /// This will never set the DefaultInstance or fire the Initialized event.
        /// </remarks>
        [Newtonsoft.Json.JsonConstructor] //< use this constructor for JSON; never set DefaultInstance
        [System.Text.Json.Serialization.JsonConstructor]
        private Configuration() => Type = GetType();
        #endregion Constructors

        #region Fields
        /// <summary>
        /// The actual type of this instance.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        protected readonly Type Type;
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        private static bool _initialized = false; //< true when at least one instance has initialized
        #endregion Fields

        #region Properties
        /// <summary>
        /// Gets the default <see cref="Configuration"/> instance.
        /// </summary>
        /// <exception cref="NotInitializedException">Get method failed because no <see cref="Configuration"/>-derived instances have been initialized yet.</exception>
        /// <returns>The default <see cref="Configuration"/>-derived object instance.</returns>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public static Configuration DefaultInstance
        {
            get => _defaultInstance ?? throw new NotInitializedException();
            protected set => _defaultInstance = value;
        }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        private static Configuration? _defaultInstance = null;
        /// <summary>
        /// Gets whether the DefaultInstance has been initialized or not.
        /// </summary>
        /// <returns><see langword="true"/> when the DefaultInstance is initialized and can be accessed; otherwise <see langword="false"/>.</returns>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public static bool HasDefaultInstance => _defaultInstance != null;
        /// <summary>
        /// Gets whether this instance is the <see cref="DefaultInstance"/> instance.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsDefaultInstance => ReferenceEquals(this, DefaultInstance);
        /// <summary>
        /// Gets whether this instance is currently setting loaded property values or not.
        /// </summary>
        /// <remarks>
        /// This can be used to prevent unnecessary PropertyChanged notifications from triggering automatic saves in derived classes.
        /// </remarks>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsLoading { get; }
        /// <summary>
        /// Gets the types to copy directly.
        /// </summary>
        /// <remarks>
        /// Types that cannot, or should not, be recursively copied should be added here.
        /// </remarks>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        protected virtual IList<Type> TypesNotToSetRecursively => new[] { typeof(string), typeof(IEnumerable) };
        /// <summary>
        /// Attribute types that, when applied to a property in this class (or a property inside of any of this class' properties), prevent <see cref="SetTo(Configuration)"/> from copying it between instances.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        protected virtual IList<Type> AttributeTypesNotToSet => Array.Empty<Type>();
        /// <summary>
        /// Whether to catch and automatically resolve exceptions that occur when recursively copying values in <see cref="SetTo(Configuration)"/>.
        /// </summary>
        /// <remarks>
        /// When <see langword="false"/>, copy operations will be much slower than they otherwise would be. You can prevent these exceptions entirely by adding the type that they occur for to <see cref="TypesNotToSetRecursively"/>.
        /// </remarks>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        protected virtual bool ThrowOnSetValueRecursivelyError => true;
        #endregion Properties

        #region Events
        /// <summary>
        /// Occurs when the first <see cref="Configuration"/>-derived instance is initialized and is ready to use.
        /// This event is only fired once.
        /// </summary>
        public static event EventHandler<Configuration>? Initialized;
        private void NotifyInitialized(Configuration instance) => Initialized?.Invoke(this, instance);
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Triggers the PropertyChanged event for the specified <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyName">The name of the property that was changed. Leave blank to get the name of the property that this method was called from.</param>
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
        /// Valid entries are the names of properties to the <see cref="Configuration"/>-derived <see langword="object"/> cfgType that is pointed from by the <see langword="static"/> <see cref="DefaultInstance"/> property.</param>
        /// <returns>The value of the property with the specified <paramref name="name"/> if it exists; otherwise <see langword="null"/>.</returns>
        [SuppressPropertyChangedWarnings]
        public virtual object? this[string name]
        {
            get => Type.GetProperty(name)?.GetValue(this);
            set => Type.GetProperty(name)?.SetValue(this, value);
        }
        #endregion Operators

        #region Methods

        #region CanSetValue
        /// <summary>
        /// Checks whether the property represented by the specified <paramref name="propertyInfo"/> can be copied between <see cref="Configuration"/> instances by <see cref="SetTo(Configuration)"/>.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> object representing a property found in this instance, or inside of one of this instance's properties.</param>
        /// <returns><see langword="true"/> when the property can be copied; otherwise <see langword="false"/>.</returns>
        protected virtual bool CanSetValue(PropertyInfo propertyInfo)
        {
            foreach (var attributeType in propertyInfo.GetCustomAttributes().Select(a => a.GetType()))
            {
                if (AttributeTypesNotToSet.Contains(attributeType))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion CanSetValue

        #region CanSetValueRecursively
        /// <summary>
        /// Checks whether the property represented by the specified <paramref name="propertyInfo"/> should have its properties recursively copied, or whether the entire property instance should be copied instead.
        /// </summary>
        /// <remarks>
        /// By default, this checks if the property type can be assigned from any one of the types in the <see cref="TypesNotToSetRecursively"/> list.
        /// </remarks>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> object representing a property found in this instance, or inside of one of this instance's properties.</param>
        /// <returns><see langword="true"/> when the property can be recursively copied; otherwise <see langword="false"/>.</returns>
        protected virtual bool CanSetValueRecursively(PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;
            foreach (var item in TypesNotToSetRecursively)
            {
                if (item.IsAssignableFrom(propertyType))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion CanSetValueRecursively

        #region SetValueRecursively
        private void SetValueRecursively(PropertyInfo propertyInfo, object source, object target, BindingFlags bindingFlags)
        {
            // check if this property is valid:
            //  - does not have any attributes that indicate the property shouldn't be copied
            //  - has a get method (for the source)
            //  - has a set method (for the target)
            //  - is not named "Item"
            if (!CanSetValue(propertyInfo) || !propertyInfo.CanRead || !propertyInfo.CanWrite || propertyInfo.Name.Equals("Item", StringComparison.Ordinal))
                return;

            var propertyType = propertyInfo.PropertyType;

            if (propertyType.IsValueType || CanSetValueRecursively(propertyInfo))
            { // property is a simple value type, an enumerable, or in DoNotRecurseTypes
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
                    // enumerate subproperties in property value
                    foreach (var subPropertyInfo in propertyType.GetProperties(bindingFlags))
                    {
                        SetValueRecursively(subPropertyInfo, sourceValue, targetValue, bindingFlags);
                    }
                }
                catch // fallback to setting property directly
                {
                    if (ThrowOnSetValueRecursivelyError) throw;
                    propertyInfo.SetValue(target, propertyInfo.GetValue(source));
                }
            }
        }
        #endregion SetValueRecursively

        #region SetTo
        /// <summary>
        /// Sets the values of all public non-static fields and properties of this instance to the values in the specified <paramref name="other"/> instance.
        /// </summary>
        /// <param name="other">Another <see cref="Configuration"/>-derived instance.</param>
        public virtual void SetTo(Configuration other)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            // enumerate properties
            foreach (var propertyInfo in Type.GetProperties(bindingFlags))
            {
                SetValueRecursively(propertyInfo, other, this, bindingFlags);
            }
        }
        #endregion SetTo

        #region Deserialize
        /// <summary>
        /// Deserializes the specified <paramref name="serializedData"/> into a new <see cref="Configuration"/> instance.
        /// </summary>
        /// <remarks>
        /// This is used by <see cref="LoadFrom(string)"/> &amp; <see cref="LoadFrom(Stream, bool, bool)"/>.
        /// </remarks>
        /// <param name="serializedData">The JSON data to deserialize as a <see cref="string"/>.</param>
        /// <returns>A new instance of this class' type from <paramref name="serializedData"/>.</returns>
        protected abstract Configuration? Deserialize(string serializedData);
        #endregion Deserialize

        #region LoadFrom
        /// <summary>
        /// Loads property values from the file at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The full path of the JSON file to read.</param>
        /// <returns><see langword="true"/> when successful; otherwise <see langword="false"/>.</returns>
        protected bool LoadFrom(string path)
        {
            if (!FileIO.TryRead(path, out string serializedData))
                return false;

            var inst = Deserialize(serializedData);

            if (inst == null) return false;

            SetTo(inst);

            if (inst is IDisposable disposable)
                disposable.Dispose(); //< dispose of derived types that implement IDisposable

            NotifyLoaded();
            return true;
        }
        /// <summary>
        /// Loads property values from the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream to read serialized JSON data from.</param>
        /// <param name="leaveStreamOpen">When <see langword="true"/>, the <paramref name="stream"/> is left open and the caller is responsible for disposing of it; when <see langword="false"/>, the stream is disposed of before returning.</param>
        /// <param name="notifyLoaded">When <see langword="true"/>, the <see cref="Loaded"/> event is fired.</param>
        /// <returns><see langword="true"/> when successful; otherwise <see langword="false"/>.</returns>
        protected bool LoadFrom(Stream stream, bool leaveStreamOpen = false, bool notifyLoaded = true)
        {
            var inst = Deserialize(FileIO.Read(stream, leaveStreamOpen));

            if (inst == null) return false;

            SetTo(inst);

            if (inst is IDisposable disposable)
                disposable.Dispose();

            if (notifyLoaded)
                NotifyLoaded();
            return true;
        }
        #endregion LoadFrom

        #region Serialize
        /// <summary>
        /// Serializes this <see cref="Configuration"/> instance and returns it as a <see cref="string"/>.
        /// </summary>
        /// <remarks>
        /// This is used by <see cref="SaveTo(string)"/> &amp; <see cref="SaveTo(Stream, bool, bool)"/>.
        /// </remarks>
        /// <returns>A <see cref="string"/> containing the serialized representation of this class instance's properties.</returns>
        protected abstract string Serialize();
        #endregion Serialize

        #region SaveTo
        /// <summary>
        /// Saves property values to the file at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The full path of the JSON file to write.</param>
        /// <returns><see langword="true"/> when successful; otherwise <see langword="false"/>.</returns>
        protected bool SaveTo(string path)
        {
            if (FileIO.TryWrite(path, Serialize()))
            {
                NotifySaved();
                return true;
            }
            else return false;
        }
        /// <summary>
        /// Saves property values to the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream to write serialized JSON data to.</param>
        /// <param name="leaveStreamOpen">When <see langword="true"/>, the <paramref name="stream"/> is left open and the caller is responsible for disposing of it; when <see langword="false"/>, the stream is disposed of before returning.</param>
        /// <param name="notifySaved">When <see langword="true"/>, the <see cref="Saved"/> event is fired.</param>
        protected void SaveTo(Stream stream, bool leaveStreamOpen = true, bool notifySaved = false)
        {
            FileIO.Write(stream, Serialize(), leaveStreamOpen: leaveStreamOpen);
            if (notifySaved)
                NotifySaved();
        }
        #endregion SaveTo

        #region Clone
        /// <inheritdoc/>
        /// <exception cref="MissingMethodException">Type does not have a parameterless constructor.</exception>
        object ICloneable.Clone() => this.Clone();
        #endregion Clone

        #endregion Methods
    }
}