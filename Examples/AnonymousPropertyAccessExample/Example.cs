using AppConfig;

namespace AnonymousPropertyAccessExample
{
    public static class Example
    {
        private static Configuration DefaultConfig => Configuration.DefaultInstance;

        /// <summary>
        /// This project is a dependency of the UsageExample project, so we can't access properties in MyConfig directly.
        /// However, we can still use the default Configuration instance's indexer to access them anonymously.
        /// </summary>
        /// <param name="propertyNames">
        /// The names of properties in <see cref="Configuration.DefaultInstance"/>.
        /// </param>
        /// <returns>
        /// The values of the properties with the specified <paramref name="propertyNames"/>.
        /// </returns>
        public static object?[] GetAndSetPropertiesAnonymously(params string[] propertyNames)
        {
            List<object?> propertyValues = new();

            // return early if the default instance hasn't been initialized yet
            if (!Configuration.HasDefaultInstance)
                return propertyValues.ToArray();

            foreach (string propertyName in propertyNames)
            {
                propertyValues.Add(DefaultConfig[propertyName]);
            }

            return propertyValues.ToArray();
        }
    }
}