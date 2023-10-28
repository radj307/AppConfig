using Newtonsoft.Json;

namespace AppConfig
{
    /// <summary>
    /// <see langword="abstract"/> extension of the <see cref="ConfigurationFile"/> class that automatically saves the config when a property value is changed.
    /// </summary>
    public abstract class ConfigurationFileWithAutosave : ConfigurationFile
    {
        #region Constructor
        /// <summary>
        /// Creates a new <see cref="ConfigurationFileWithAutosave"/> instance with the specified <paramref name="location"/> &amp; <paramref name="initialAutosaveState"/>.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="initialAutosaveState"></param>
        protected ConfigurationFileWithAutosave(string location, bool initialAutosaveState = false) : base(location)
        {
            EnableAutosave = initialAutosaveState;
        }
        #endregion Constructor

        #region Fields
        private bool _hasLoaded = false;
        #endregion Fields

        #region Properties
        /// <summary>
        /// Gets or sets whether the config is automatically saved whenever a property is changed.
        /// </summary>
        [JsonIgnore]
        public bool EnableAutosave
        {
            get => _autosaveEnabled;
            set
            {
                _autosaveEnabled = value;

                if (_autosaveEnabled)
                {
                    EnableAutosaving();
                }
                else
                {
                    DisableAutosaving();
                }

                NotifyPropertyChanged();
            }
        }
        [JsonIgnore]
        private bool _autosaveEnabled;
        #endregion Properties

        #region Methods
        /// <summary>
        /// Enables automatic saving of the config when a property value is changed.
        /// </summary>
        protected virtual void EnableAutosaving()
        {
            PropertyChanged += ConfigurationFileWithAutosave_PropertyChanged;
        }
        /// <summary>
        /// Disables automatic saving of the config when a property value is changed.
        /// </summary>
        protected virtual void DisableAutosaving()
        {
            PropertyChanged -= ConfigurationFileWithAutosave_PropertyChanged;
        }
        #endregion Methods

        #region Method Overrides
        /// <inheritdoc/>
        public override bool Load()
        {
            var result = base.Load();
            _hasLoaded = true;
            return result;
        }
        #endregion Method Overrides

        #region EventHandlers

        #region ConfigurationFileWithAutosave
        private void ConfigurationFileWithAutosave_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (IsLoading || !_hasLoaded) return;

            Save();
        }
        #endregion ConfigurationFileWithAutosave

        #endregion EventHandlers
    }
}
