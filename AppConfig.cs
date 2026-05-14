namespace media_overlay;

/// <summary>
/// Represents the application configuration loaded from config.yaml.
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Gets or sets the display settings for the overlay.
    /// </summary>
    public DisplayConfig Display { get; set; } = new();

    /// <summary>
    /// Gets or sets the format string for displaying media information.
    /// </summary>
    public string Format { get; set; } = "{Artist} - {Title}";

    /// <summary>
    /// Gets or sets the media filter configuration.
    /// </summary>
    public MediaFilterConfig MediaFilter { get; set; } = new();
}

/// <summary>
/// Defines screen corner positions for the overlay.
/// </summary>
public enum ScreenCorner
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

/// <summary>
/// Display configuration for the overlay.
/// </summary>
public class DisplayConfig
{
    /// <summary>
    /// Gets or sets the monitor index (0 = primary, 1 = secondary, etc.).
    /// Default is the last monitor.
    /// </summary>
    public int MonitorIndex { get; set; } = -1; // -1 means last monitor

    /// <summary>
    /// Gets or sets the screen corner where the overlay is positioned.
    /// Default is TopRight.
    /// </summary>
    public ScreenCorner Corner { get; set; } = ScreenCorner.TopRight;

    /// <summary>
    /// Gets or sets the width of the overlay window in pixels.
    /// </summary>
    public int Width { get; set; } = 600;

    /// <summary>
    /// Gets or sets the height of the overlay window in pixels.
    /// </summary>
    public int Height { get; set; } = 43;

    /// <summary>
    /// Gets or sets the text color as an RGB hex string (e.g., "FFFFFF" for white).
    /// </summary>
    public string TextColor { get; set; } = "FFFFFF";

    /// <summary>
    /// Gets or sets the text outline color as an RGB hex string (e.g., "000000" for black).
    /// </summary>
    public string OutlineColor { get; set; } = "000000";

    /// <summary>
    /// Gets or sets the text outline width in pixels.
    /// </summary>
    public int OutlineWidth { get; set; } = 2;

    /// <summary>
    /// Gets or sets the font size for the text.
    /// </summary>
    public float FontSize { get; set; } = 24f;

    /// <summary>
    /// Gets or sets the font family name.
    /// </summary>
    public string FontFamily { get; set; } = "Segoe UI";
}

/// <summary>
/// Media filter configuration to include/exclude specific programs.
/// </summary>
public class MediaFilterConfig
{
    /// <summary>
    /// Gets or sets the list of program names to exclude.
    /// </summary>
    public List<string> ExcludePrograms { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of program names to include.
    /// If empty, all programs are included (except excluded ones).
    /// </summary>
    public List<string> IncludePrograms { get; set; } = new();
}
