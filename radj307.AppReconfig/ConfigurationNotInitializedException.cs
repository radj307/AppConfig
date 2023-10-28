using System;

namespace AppConfig
{
    /// <summary>
    /// The exception that is thrown when the default <see cref="Configuration"/> was accessed before any instances were initialized.
    /// </summary>
    public sealed class NotInitializedException : Exception
    {
        internal NotInitializedException() : base($"At least one {nameof(Configuration)}-derived instance must be initialized before accessing {nameof(Configuration)}.Default!") { }
    }
}