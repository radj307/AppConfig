using Newtonsoft.Json;
using System;
using System.IO;

namespace AppConfig
{
    /// <summary>
    /// <see langword="abstract"/> extension of the <see cref="ConfigurationJson"/> class that maintains the file location and provides methods to save to and load from it.
    /// </summary>
    [Serializable]
    public abstract class ConfigurationFile : ConfigurationJson
    {
        #region Constructors
        /// <summary>
        /// Creates a new <see cref="ConfigurationFile"/> instance using the specified <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The location of the JSON file in the filesystem.</param>
        protected ConfigurationFile(string location) : base() => Location = location;
        #endregion Constructors

        #region Properties
        /// <summary>
        /// The location of the JSON configuration file in the local filesystem.
        /// </summary>
        [JsonIgnore]
        public string Location { get; set; }
        /// <inheritdoc/>
        protected override JsonSerializerSettings JsonSerializerSettings => new() { Formatting = Formatting.Indented };
        #endregion Properties

        #region Methods
        /// <summary>
        /// Loads config values from the JSON file specified by <see cref="Location"/>
        /// </summary>
        /// <remarks>This method may be overloaded in derived classes.</remarks>
        /// <returns><see langword="true"/> when the file specified by <see name="Location"/> exists and was successfully loaded; otherwise <see langword="false"/>.</returns>
        public virtual bool Load()
            => LoadFrom(Location);
        /// <inheritdoc cref="Configuration.LoadFrom(Stream, bool, bool)"/>
        public bool LoadFrom(Stream stream, bool leaveStreamOpen = false) => LoadFrom(stream, leaveStreamOpen);
        /// <summary>
        /// Saves config values to the JSON file specified by <see name="Location"/>
        /// </summary>
        /// <remarks>This method may be overloaded in derived classes.</remarks>
        /// <param name="formatting">Formatting type to use when serializing this class instance.</param>
        public virtual void Save(Formatting formatting = Formatting.Indented)
            => SaveTo(Location);
        #endregion Methods
    }
}
