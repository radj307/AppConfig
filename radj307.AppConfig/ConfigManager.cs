using System.ComponentModel;

namespace AppConfig
{
    /// <summary>
    /// Wrapper object that manages an <see cref="ConfigBase"/> instance, and implements saving/loading functionality for it.
    /// </summary>
    /// <typeparam name="T">Typename of a class that implements <see cref="ConfigBase"/> and is default constructible.</typeparam>
    public class ConfigManager<T> : INotifyPropertyChanged, INotifyPropertyChanging where T : ConfigBase, new()
    {
        #region Constructor
        /// <summary>
        /// Creates a new <see cref="ConfigManager{T}"/> instance using the given <paramref name="loader"/>.
        /// </summary>
        /// <param name="loader">Object that implements the save/load mechanism to <see cref="ILoader"/>.</param>
        public ConfigManager(ILoader loader) => _loader = loader;
        /// <summary>
        /// Creates a new <see cref="ConfigManager{T}"/> instance using a <see cref="JsonLoader"/> &amp; a given <paramref name="path"/>.
        /// </summary>
        /// <param name="path"></param>
        public ConfigManager(string path) => _loader = new JsonLoader(path);
        #endregion Constructor

        #region Fields
        private bool _loading = false;
        #endregion Fields

        #region Properties
        /// <summary>
        /// Creates and returns a new instance of type <typeparamref name="T"/>.<br/>
        /// Event forwards are automatically applied.
        /// </summary>
        private T NewInst
        {
            get
            {
                T inst = new();
                inst.PropertyChanging += Inst_PropertyChanging;
                inst.PropertyChanged += Inst_PropertyChanged;
                return inst;
            }
        }
        /// <summary>
        /// Configuration instance containing the data from save/load.
        /// </summary>
        public T Inst
        {
            get => _inst ??= NewInst;
            set
            {
                if (value.Equals(_inst)) return;

                if (_inst is null)
                    _inst = NewInst;

                _inst.CopyFrom(value);
            }
        }
        private T? _inst;
        /// <summary>
        /// The default save/load loader instance.<br/>Used by the <see cref="Save"/> &amp; <see cref="Load"/> methods.
        /// </summary>
        public ILoader Loader
        {
            get => _loader;
            set
            {
                if (value.Equals(_loader)) return;

                _loader = value;
            }
        }
        private ILoader _loader;
        /// <summary>
        /// Gets or sets whether auto-saving is enabled.<br/>
        /// When <see langword="true"/>, the config is automatically saved using the <see cref="Loader"/> whenever a property is changed.
        /// </summary>
        /// <remarks><b>Default: <see langword="false"/></b></remarks>
        public bool AutoSave { get; set; } = false;
        /// <summary>
        /// Gets or sets whether PropertyChanging/PropertyChanged events triggered by <see cref="Inst"/> are re-triggered by the manager instance.
        /// </summary>
        /// <remarks><b>Default: <see langword="true"/></b></remarks>
        public bool ForwardPropertyChangeEvents { get; set; } = true;
        #endregion Properties

        #region Events
        /// <summary>
        /// Triggered when the value of a member property was changed.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Triggers the <see cref="PropertyChanged"/> event.
        /// </summary>
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new(propertyName));
        private void Inst_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!_loading && AutoSave)
                Save();
            if (ForwardPropertyChangeEvents)
                PropertyChanged?.Invoke(sender, e);
        }
        /// <summary>
        /// Triggered just before the value of a member property is changed.
        /// </summary>
        public event PropertyChangingEventHandler? PropertyChanging;
        /// <summary>
        /// Triggers the <see cref="PropertyChanging"/> event.
        /// </summary>
        protected void OnPropertyChanging(string propertyName) => PropertyChanging?.Invoke(this, new(propertyName));
        private void Inst_PropertyChanging(object? sender, PropertyChangingEventArgs e)
        {
            if (!_loading && AutoSave)
                Save();
            if (ForwardPropertyChangeEvents)
                PropertyChanging?.Invoke(sender, e);
        }
        #endregion Events

        #region Methods
        /// <summary>
        /// (re)sets the <see cref="Inst"/> from the values in the given <paramref name="defaultInstance"/>.
        /// </summary>
        /// <param name="defaultInstance">An instance from use as the copy source when resetting.</param>
        public void Reset(T defaultInstance) => Inst.CopyFrom(defaultInstance);
        /// <inheritdoc cref="Reset(T)"/>
        public void Reset() => Reset(Activator.CreateInstance<T>());
        /// <summary>
        /// Saves the configuration from the default loader.
        /// </summary>
        public void Save() => Loader.Save(Inst);
        /// <summary>
        /// Loads the configuration to the default loader.
        /// </summary>
        public void Load()
        {
            _loading = true;
            Loader.Load(Inst);
            _loading = false;
        }
        #endregion Methods
    }
}