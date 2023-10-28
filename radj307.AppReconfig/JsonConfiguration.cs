using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AppConfig
{
    /// <summary>
    /// <see langword="abstract"/> <see cref="Configuration"/> class with JSON support.
    /// </summary>
    [Serializable]
    public abstract class JsonConfiguration : Configuration
    {
        #region Properties
        /// <summary>
        /// The default JsonSerializerSettings object to use when serializing JSON data.
        /// </summary>
        [JsonIgnore]
        protected virtual JsonSerializerSettings JsonSerializerSettings { get; } = new();
        /// <summary>
        /// The default JsonConverter objects to use when deserializing JSON data.
        /// </summary>
        /// <remarks>
        /// By default, this uses the Converters from the JsonSerializerSettings property.
        /// </remarks>
        [JsonIgnore]
        protected virtual IList<JsonConverter> JsonConverters => JsonSerializerSettings.Converters;
        /// <inheritdoc/>
        protected override IList<Type> AttributeTypesNotToSet => new[] { typeof(JsonIgnoreAttribute) };
        #endregion Properties

        #region Configuration Method Overrides
        /// <inheritdoc/>
        protected override Configuration? Deserialize(string serializedData) => (Configuration?)JsonConvert.DeserializeObject(serializedData, Type, JsonSerializerSettings);
        /// <inheritdoc cref="Serialize()"/>
        /// <param name="formatting">The JSON formatting type to use.</param>
        protected string Serialize(Formatting formatting) => JsonConvert.SerializeObject(this, Type, formatting, JsonSerializerSettings);
        /// <inheritdoc/>
        protected override string Serialize() => Serialize(JsonSerializerSettings.Formatting);
        #endregion Configuration Method Overrides
    }
}