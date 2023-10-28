using System.IO;

namespace AppConfig
{
    internal static class FileIO
    {
        #region Read
        internal static string Read(Stream stream, bool leaveStreamOpen)
        {
            using var reader = new StreamReader(stream, leaveOpen: leaveStreamOpen);
            string contents = reader.ReadToEnd();
            reader.Dispose();
            return contents;
        }
        #endregion Read

        #region TryRead
        /// <summary>
        /// Attempts to read the file at the specified <paramref name="path"/> and retrieves its <paramref name="contents"/>.
        /// </summary>
        /// <param name="path">The full path to the target file.</param>
        /// <param name="contents">The contents of the file located at <paramref name="path"/> when successful; otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> when successful and <paramref name="contents"/> is not <see langword="null"/>; otherwise <see langword="false"/>.</returns>
        internal static bool TryRead(string path, out string contents)
        {
            if (!File.Exists(path))
            {
                contents = null!;
                return false;
            }

            try
            {
                contents = Read(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), leaveStreamOpen: false);
                return true;
            }
            catch
            {
                contents = null!;
                return false;
            }
        }
        #endregion TryRead

        #region Write
        internal static void Write(Stream stream, string data, bool leaveStreamOpen = false)
        {
            using var writer = new StreamWriter(stream, System.Text.Encoding.UTF8, leaveOpen: leaveStreamOpen);
            writer.Write(data);
            writer.Flush();
            writer.Dispose();
        }
        #endregion Write

        #region TryWrite
        /// <summary>
        /// Attempts to write <paramref name="content"/> to the file at the specified <paramref name="path"/>.
        /// </summary>
        /// <remarks>
        /// This method writes to a temp file, then moves it to the target location to prevent blocking for extended periods of time.
        /// </remarks>
        /// <param name="path">The full path to the target file.</param>
        /// <param name="content">The data to write to the file.</param>
        /// <returns><see langword="true"/> when successful; otherwise <see langword="false"/>.</returns>
        internal static bool TryWrite(string path, string content)
        {
            var tempPath = Path.GetTempFileName();
            Write(File.Open(tempPath, FileMode.Open, FileAccess.Write), content);

            try
            {
                File.Move(tempPath, path, overwrite: true); //< original temp file is deleted
                return true;
            }
            catch
            {
                File.Delete(tempPath);
                return false;
            }
        }
        #endregion TryWrite
    }
}