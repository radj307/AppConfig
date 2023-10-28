using System;
using UltraMapper;

namespace AppConfig
{
    /// <summary>
    /// AppConfig extension methods.
    /// </summary>
    public static class ConfigurationExtensions
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
    }
}