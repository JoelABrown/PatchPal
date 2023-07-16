using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mooseware.PatchPal
{
    /// <summary>
    /// Abstract class for reading (and writing) serialized POCO objects of type T from JSON settings files.
    /// To implement this in a POCO settings structure, declare the POCO as follows:
    ///     internal class PoCo : SettingsManager<PoCo> {...}
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SettingsManager<T> where T : SettingsManager<T>, new()
    {
        /// <summary>
        /// Full path and file spec of the JSON settings file
        /// </summary>
        private static readonly string filePath = GetLocalFilePath($"{typeof(T).Name}.json");

        public static T? Settings { get; private set; }

        /// <summary>
        /// Constructs the full path and file specification of the settings JSON file based on the given file name and assembly information.
        /// The file will be located in the local app data folder, under the organization and product name folders (based on the assembly info)
        /// </summary>
        /// <param name="fileName">The file name (including extension) to use for the settings file.</param>
        /// <returns>The full path and file spec of the settings file</returns>
        private static string GetLocalFilePath(string fileName)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var orgName = Assembly.GetEntryAssembly()?.GetCustomAttributes<AssemblyCompanyAttribute>().FirstOrDefault();
            var prodName = Assembly.GetEntryAssembly()?.GetCustomAttributes<AssemblyProductAttribute>().FirstOrDefault();
            return Path.Combine(appData,
                orgName?.Company ?? MethodInfo.GetCurrentMethod()?.ReflectedType?.Namespace ?? string.Empty,
                prodName?.Product ?? Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty,
                fileName);
        }

        /// <summary>
        /// Loads the settings from the JSON file.
        /// </summary>
        public static void Load()
        {
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                Settings = System.Text.Json.JsonSerializer.Deserialize<T>(content) ?? new T();
            }
            else
            {
                // Try loading from the default settings distributed with the application instead...
                string defaultPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
                string fileName = $"{typeof(T).Name}.json";
                string defaultSettingsFile = Path.Combine(defaultPath, fileName);

                if (File.Exists(defaultSettingsFile))
                {
                    string content = File.ReadAllText(defaultSettingsFile);
                    Settings = System.Text.Json.JsonSerializer.Deserialize<T>(content) ?? new T();
                }
                else
                {
                    Settings = new T();
                }
            }
        }

        /// <summary>
        /// Saves the current settings values to a JSON file.
        /// </summary>
        public static void Save()
        {
            string json = System.Text.Json.JsonSerializer.Serialize(Settings);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);
            File.WriteAllText(filePath, json);
        }
    }
}
