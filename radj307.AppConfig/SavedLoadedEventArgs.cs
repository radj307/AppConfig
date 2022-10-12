namespace AppConfig
{
    /// <summary>
    /// Event argument type for the <see cref="JsonLoader.Saved"/> &amp; <see cref="JsonLoader.Loaded"/> events.
    /// </summary>
    public class SavedLoadedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new <see cref="SavedLoadedEventArgs"/> instance.
        /// </summary>
        /// <param name="path"><see cref="Path"/></param>
        /// <param name="successful"><see cref="Success"/></param>
        public SavedLoadedEventArgs(string path, bool successful)
        {
            Path = path;
            Success = successful;
        }
        /// <inheritdoc cref="JsonLoader.Path"/>
        public string Path { get; }
        /// <summary>
        /// <see langword="true"/> when the operation completed successfully; otherwise <see langword="false"/>.
        /// </summary>
        public bool Success { get; }
    }
}