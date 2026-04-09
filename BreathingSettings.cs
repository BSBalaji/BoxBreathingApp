using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoxBreathingTray
{
    /// <summary>
    /// Holds all user-configurable settings and handles load/save to AppData.
    /// </summary>
    public class BreathingSettings
    {
        // Seconds per side (box breathing default: 4)
        public int SecondsPerSide { get; set; } = 4;

        // Colors stored as ARGB ints for JSON serialization
        public int SquareColorArgb { get; set; } = Color.FromArgb(80, 120, 200).ToArgb();
        public int DotColorArgb { get; set; } = Color.FromArgb(255, 255, 255).ToArgb();
        public int BackgroundColorArgb { get; set; } = Color.FromArgb(20, 20, 30).ToArgb();

        [JsonIgnore]
        public Color SquareColor
        {
            get => Color.FromArgb(SquareColorArgb);
            set => SquareColorArgb = value.ToArgb();
        }

        [JsonIgnore]
        public Color DotColor
        {
            get => Color.FromArgb(DotColorArgb);
            set => DotColorArgb = value.ToArgb();
        }

        [JsonIgnore]
        public Color BackgroundColor
        {
            get => Color.FromArgb(BackgroundColorArgb);
            set => BackgroundColorArgb = value.ToArgb();
        }

        // ── Persistence ────────────────────────────────────────────────────

        private static string SettingsPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BoxBreathingTray",
                "settings.json");

        public static BreathingSettings LoadOrDefault()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<BreathingSettings>(json)
                           ?? new BreathingSettings();
                }
            }
            catch { /* fall through to default */ }
            return new BreathingSettings();
        }

        public void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(SettingsPath)!;
                Directory.CreateDirectory(dir);
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch { /* silent — settings are non-critical */ }
        }
    }
}
