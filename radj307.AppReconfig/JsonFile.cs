using Newtonsoft.Json;
using System;
using System.IO;

namespace AppConfig
{
    /// <summary>
    /// Wraps JSON serialization and file I/O for the <see cref="Configuration"/> class.
    /// </summary>
    internal static class JsonFile
    {
        #region Load
        /// <summary>
        /// Load the file specified by <paramref name="path"/> into a new type specified by <paramref name="type"/>.
        /// </summary>
        /// <remarks>
        /// The file encoding is automatically determined from byte order marks.
        /// </remarks>
        /// <param name="path">The location of the JSON file to read.</param>
        /// <param name="type">The type of object to deserialize the JSON file into.</param>
        /// <returns>An <see langword="object"/> of the specified <paramref name="type"/>; or <see langword="null"/> if the file doesn't exist or contains incompatible data.</returns>
        public static object? Load(string path, Type type, JsonConverter[] jsonConverters)
        {
            if (!File.Exists(path))
                return null;
            using var reader = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), System.Text.Encoding.UTF8, true);
            string content = reader.ReadToEnd();
            reader.Dispose();
            try
            {
                return JsonConvert.DeserializeObject(content, type, jsonConverters);
            }
            catch
            {
                return null;
            }
        }
        #endregion Load

        #region Save
        /// <summary>
        /// Save a <see cref="Configuration"/>-derived <paramref name="instance"/> to the file specified by <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The location of the JSON file to write.</param>
        /// <param name="instance">A <see cref="Configuration"/>-derived <see langword="class"/> to write.</param>
        /// <param name="formatting">Formatting type to use when serializing the object <paramref name="instance"/>.</param>
        /// <param name="jsonSerializerSettings">The <see cref="JsonSerializerSettings"/> to use when serializing the config.</param>
        /// <param name="deleteTempFile"><see langword="true"/> cleans up the temp file; <see langword="false"/> does not.</param>
        public static bool Save(string path, Configuration instance, Formatting formatting, JsonSerializerSettings jsonSerializerSettings, bool deleteTempFile)
        {
            string serialized = JsonConvert.SerializeObject(instance, formatting, jsonSerializerSettings);
            string tempFilePath = Path.GetTempFileName();
            using (var writer = new StreamWriter(File.Open(tempFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None), System.Text.Encoding.UTF8))
            {
                writer.Write(serialized);
                writer.Flush();
            };
            try
            {
                File.Move(tempFilePath, path, true);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (deleteTempFile)
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch { } //< at least we tried
                }
            }
        }
        #endregion Save
    }
}