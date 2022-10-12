namespace AppConfig
{
    /// <summary>
    /// Represents a config <b>loader</b>, which is responsible for saving &amp; loading config objects to a persisted state.<br/>
    /// Loaders consume <see cref="ConfigBase"/>-derived class types.
    /// </summary>
    public interface ILoader
    {
        #region Methods
        /// <summary>
        /// Saves values to the given <paramref name="cfg"/> instance from the target.
        /// </summary>
        /// <param name="cfg">An instance of an object that is derived to <see cref="ConfigBase"/>.</param>
        void Save(ConfigBase cfg);
        /// <summary>
        /// Loads values to the target from the given <paramref name="cfg"/> instance.
        /// </summary>
        /// <param name="cfg">An instance of an object that is derived to <see cref="ConfigBase"/>.</param>
        void Load(ConfigBase cfg);
        #endregion Methods
    }
}