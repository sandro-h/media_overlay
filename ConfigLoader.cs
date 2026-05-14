using System.IO;
using System.Globalization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace media_overlay;

/// <summary>
/// Handles loading and managing application configuration.
/// </summary>
public static class ConfigLoader
{
    private static readonly string ConfigPath = Path.Combine(
        AppContext.BaseDirectory, 
        "config.yaml"
    );

    private static AppConfig? _cachedConfig;

    /// <summary>
    /// Gets the current application configuration.
    /// Loads from config.yaml if not already cached.
    /// </summary>
    public static AppConfig GetConfig()
    {
        if (_cachedConfig != null)
            return _cachedConfig;

        if (File.Exists(ConfigPath))
        {
            Console.WriteLine($"Loading config from ${ConfigPath}");
            try
            {
                var yaml = File.ReadAllText(ConfigPath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                _cachedConfig = deserializer.Deserialize<AppConfig>(yaml) ?? new AppConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config.yaml: {ex.Message}. Using defaults.");
                _cachedConfig = new AppConfig();
            }
        }
        else
        {
            // Create a default config file
            Console.WriteLine($"${ConfigPath} not found. Using default config");
            CreateDefaultConfigFile();
            _cachedConfig = new AppConfig();
        }

        return _cachedConfig;
    }

    /// <summary>
    /// Creates a default config.yaml file with example settings.
    /// </summary>
    private static void CreateDefaultConfigFile()
    {
        try
        {
            var defaultConfig = new AppConfig();
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(defaultConfig);

            // Add helpful comments to the generated YAML
            var yamlWithComments = $@"# Media Overlay Configuration
# This file configures the media overlay display, format, and filtering

{yaml}";

            File.WriteAllText(ConfigPath, yamlWithComments);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating default config.yaml: {ex.Message}");
        }
    }

    /// <summary>
    /// Reloads the configuration from disk.
    /// </summary>
    public static void ReloadConfig()
    {
        _cachedConfig = null;
        GetConfig();
    }

    /// <summary>
    /// Converts a hex color string to Color object.
    /// </summary>
    public static Color HexToColor(string hexColor)
    {
        hexColor = hexColor.TrimStart('#');
        if (hexColor.Length == 6)
        {
            return Color.FromArgb(
                int.Parse(hexColor.Substring(0, 2), NumberStyles.HexNumber),
                int.Parse(hexColor.Substring(2, 2), NumberStyles.HexNumber),
                int.Parse(hexColor.Substring(4, 2), NumberStyles.HexNumber)
            );
        }
        return Color.White;
    }
}
