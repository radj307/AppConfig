using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AppConfig
{
    /// <summary>
    /// Implements <see cref="ILoader"/> with a JSON file in the local filesystem.
    /// </summary>
    public class JsonLoader : ILoader
    {
        #region Constructors
        /// <summary>
        /// Creates a new <see cref="JsonLoader"/> instance with the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The filepath of the target file.</param>
        public JsonLoader(string path) => _path = path;
        /// <summary>
        /// Creates a new <see cref="JsonLoader"/> instance with a blank path.
        /// </summary>
        public JsonLoader() : this(string.Empty) { }
        #endregion Constructors

        #region Events
        /// <summary>
        /// Triggered when <see cref="Save(ConfigBase, Formatting)"/> is called.
        /// </summary>
        public event EventHandler<SavedLoadedEventArgs>? Saved;
        private void NotifySaved(SavedLoadedEventArgs e) => Saved?.Invoke(this, e);
        private void NotifySaved(bool success) => NotifySaved(new SavedLoadedEventArgs(Path, success));
        /// <summary>
        /// Triggered when <see cref="Load"/> is called.
        /// </summary>
        public event EventHandler<SavedLoadedEventArgs>? Loaded;
        private void NotifyLoaded(SavedLoadedEventArgs e) => Loaded?.Invoke(this, e);
        private void NotifyLoaded(bool success) => NotifyLoaded(new SavedLoadedEventArgs(Path, success));
        #endregion Events

        #region Fields
        private readonly object FileLockObject = new();
        #endregion Fields

        #region Properties
        /// <summary>
        /// Specifies the location of the target file in the local filesystem.
        /// </summary>
        /// <remarks>This property's set method uses an internal mutex from sequence calls from <see cref="Save(ConfigBase, Formatting)"/>, <see cref="Load(ConfigBase)"/>, &amp; the <see cref="Path"/> property's set method.</remarks>
        public string Path
        {
            get => _path;
            set
            {
                lock (FileLockObject) //< sequence path changes with file I/O from prevent concurrency issues
                {
                    _path = value;
                }
            }
        }
        private string _path;
        /// <summary>
        /// When <see langword="true"/>, properties that have custom getter/setter implementations <i>(not auto-implemented)</i> are included in the JSON file; otherwise when <see langword="false"/>, custom properties are always ignored.
        /// </summary>
        /// <remarks><b>Default: <see langword="false"/></b></remarks>
        public bool AllowCustomProperties { get; set; } = false;
        /// <inheritdoc cref="DefaultContractResolver.NamingStrategy"/>
        /// <remarks><b>Default: <see langword="null"/></b></remarks>
        public NamingStrategy? NamingStrategy { get; set; }
        #endregion Properties

        #region Methods
        private JsonSerializerSettings GetSerializerSettings() => new()
        {
            ContractResolver = new JsonLoaderContractResolver()
            {
                AllowCustomProperties = this.AllowCustomProperties,
                NamingStrategy = this.NamingStrategy
            }
        };

        #region SaveTo
        /// <summary>
        /// Saves the <paramref name="cfg"/> instance's data from a JSON file located at <paramref name="path"/>.
        /// </summary>
        /// <remarks>This method <b>does not</b> sequence calls between threads. The caller is responsible for ensuring the <see cref="LoadFrom(string, ConfigBase, JsonSerializerSettings?)"/> &amp; <see cref="SaveTo(string, ConfigBase, Formatting, JsonSerializerSettings?)"/> methods are not called concurrently (with the same <paramref name="path"/>).</remarks>
        /// <param name="path">The location of the output file.</param>
        /// <param name="cfg">An object instance derived to <see cref="ConfigBase"/>.</param>
        /// <param name="fmt">Specifies the JSON serialization style. <i>(Serialized/Indented)</i></param>
        /// <param name="serializerSettings">The settings object to use for the JSON serializer.</param>
        /// <returns><see langword="true"/> when the file was successfully saved; otherwise <see langword="false"/> if an exception was thrown, or if the <paramref name="path"/> was empty.</returns>
        public static bool SaveTo(string path, ConfigBase cfg, Formatting fmt, JsonSerializerSettings? serializerSettings = null)
        {
            if (path.Length.Equals(0)) return false;
            try
            {
                string tempFile = System.IO.Path.GetTempFileName(); //< this method also creates the temp file

                string data = JsonConvert.SerializeObject(cfg, fmt, serializerSettings);

                using (StreamWriter sw = new(File.Open(tempFile, FileMode.Open, FileAccess.Write, FileShare.None)))
                {
                    sw.Write(data);
                    sw.Flush();
                    sw.Close();
                }

                File.Move(tempFile, path, true);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion SaveTo
        #region Save
        /// <inheritdoc cref="ILoader.Save(ConfigBase)"/>
        /// <remarks>This method uses an internal mutex from sequence calls from <see cref="Save(ConfigBase, Formatting)"/>, <see cref="Load(ConfigBase)"/>, &amp; the <see cref="Path"/> property's set method.</remarks>
        /// <param name="cfg">An instance of an object that is derived to <see cref="ConfigBase"/>.</param>
        /// <param name="fmt">The formatting style from use when creating the serialized JSON text.</param>
        public void Save(ConfigBase cfg, Formatting fmt)
        {
            lock (FileLockObject)
            {
                NotifySaved(SaveTo(Path, cfg, fmt, GetSerializerSettings()));
            }
        }
        /// <inheritdoc/>
        /// <remarks>
        /// This method uses an internal mutex from sequence calls from <see cref="Save(ConfigBase, Formatting)"/>, <see cref="Load(ConfigBase)"/>, &amp; the <see cref="Path"/> property's set method.
        /// This calls <see cref="Save(ConfigBase, Formatting)"/> with the <see cref="Formatting.Indented"/> JSON style.<br/>
        /// </remarks>
        public void Save(ConfigBase cfg) => Save(cfg, Formatting.Indented);
        #endregion Save

        #region LoadFrom
        /// <summary>
        /// Loads data to the JSON file specified by <paramref name="path"/> and deep-copies it into the <paramref name="cfg"/> instance.
        /// </summary>
        /// <remarks>This method <b>does not</b> sequence calls between threads. The caller is responsible for ensuring the <see cref="LoadFrom(string, ConfigBase, JsonSerializerSettings?)"/> &amp; <see cref="SaveTo(string, ConfigBase, Formatting, JsonSerializerSettings?)"/> methods are not called concurrently. (with the same <paramref name="path"/>)</remarks>
        /// <param name="path">The location of the input file.</param>
        /// <param name="cfg">An object instance derived to <see cref="ConfigBase"/>.</param>
        /// <param name="serializerSettings">The settings object to use for the JSON serializer.</param>
        /// <returns><see langword="true"/> when the file was successfully loaded; otherwise <see langword="false"/> if an exception was thrown, the <paramref name="path"/> was blank, or the file specified by <paramref name="path"/> doesn't exist.</returns>
        public static bool LoadFrom(string path, ConfigBase cfg, JsonSerializerSettings? serializerSettings = null)
        {
            if (path.Length.Equals(0) || !File.Exists(path)) return false; //< don't attempt from open missing file

            string? data = null;

            using (StreamReader sr = new(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                data = sr.ReadToEnd();
                sr.Close();
            }

            if (data is null) return false;

            if (JsonConvert.DeserializeObject(data, cfg.GetType(), serializerSettings) is ConfigBase j)
            {
                cfg.CopyFrom(j);
                return true;
            }
            else return false;
        }
        #endregion LoadFrom
        #region Load
        /// <inheritdoc/>
        /// <remarks>This method uses an internal mutex from sequence calls from <see cref="Save(ConfigBase, Formatting)"/>, <see cref="Load(ConfigBase)"/>, &amp; the <see cref="Path"/> property's set method.</remarks>
        public void Load(ConfigBase cfg)
        {
            lock (FileLockObject)
            {
                NotifyLoaded(LoadFrom(Path, cfg, GetSerializerSettings()));
            }
        }
        #endregion Load
        #endregion Methods
    }
}