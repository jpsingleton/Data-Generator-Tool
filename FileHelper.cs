using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace DataGenerator
{
    /// <summary>
    /// A helper class which provides methods to work with files.
    /// </summary>
    public static class FileHelper
    {
        #region methods

        /// <summary>
        /// Get a file path with an open file dialog.
        /// </summary>
        /// <returns>The file path or blank if cancelled.</returns>
        public static string getFilePathWithOpenFileDialog()
        {
            // If the user selected a file return the path
            // otherwise return a blank string.
            return getFilePathWithOpenFileDialog(string.Empty, "*");
        }

        /// <summary>
        /// Get a file path with an open file dialog.
        /// </summary>
        /// <param name="currentFileName">The old path (returned if cancelled).</param>
        /// <returns>The new or original file path if cancelled.</returns>
        public static string getFilePathWithOpenFileDialog(string originalFilePath, string FILE_EXTENSION)
        {
            string FILE_FILTER = FILE_EXTENSION.ToUpper() + " File |*." + FILE_EXTENSION.ToLower();

            // Create a new open file dialog.
            OpenFileDialog ofd = new OpenFileDialog();
            // Set the default extension.
            ofd.DefaultExt = FILE_EXTENSION;
            // Set the display filter.
            ofd.Filter = FILE_FILTER;
            // If the user selected a file.
            if ((bool)ofd.ShowDialog())
            {
                // Return the new file path
                return ofd.FileName;
            }
            else
            {
                // Return the original path.
                return originalFilePath;
            }
        }

        /// <summary>
        /// Generates a new file name
        /// </summary>
        /// <param name="fileName">File path to modify</param>
        /// <param name="addition">String to add to the end of the filename</param>
        /// <returns>The modified file path</returns>
        public static string genNewFilePath(string filePath, string addition, string FILE_EXTENSION)
        {
            return filePath.Replace(FILE_EXTENSION, " " + addition + FILE_EXTENSION);
        }

        private static bool isPathValid(string path)
        {
            // Check for invalid characters in path.
            foreach (char invalidPathChar in Path.GetInvalidPathChars())
            {
                if (path.Contains(invalidPathChar))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if a file is OK to read from. (Valid path and exists)
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <returns>True if valid, false if not.</returns>
        public static bool readOK(string path)
        {
            if (isPathValid(path))
            {
                // Return if file exists.
                return File.Exists(path);
            }
            return false;
        }

        /// <summary>
        /// Checks if a file is OK to create. (Valid path and does not exist)
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <returns>True if valid, false if not.</returns>
        public static bool newOK(string path)
        {
            if (isPathValid(path))
            {
                // Return if file does not exist.
                return !File.Exists(path);
            }
            return false;
        }

        #endregion

    }
}
