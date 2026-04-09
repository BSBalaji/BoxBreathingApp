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
        // Seconds per side for dot movement (box breathing default: 4.0)
        public double SecondsPerSide { get; set; } = 4.0;

        // Colors stored as ARGB ints for JSON serialization
        // Legacy setting kept for backward compatibility with older settings.json files.
        public int SquareColorArgb { get; set; } = Color.FromArgb(80, 120, 200).ToArgb();
        public int HorizontalSidesColorArgb { get; set; } = Color.FromArgb(80, 120, 200).ToArgb(); // top + bottom
        public int VerticalSidesColorArgb { get; set; } = Color.FromArgb(120, 90, 200).ToArgb();   // left + right
        public int DotColorArgb { get; set; } = Color.FromArgb(255, 255, 255).ToArgb();
        public int BackgroundColorArgb { get; set; } = Color.FromArgb(20, 20, 30).ToArgb();

        [JsonIgnore]
        public Color SquareColor
        {
            get => Color.FromArgb(SquareColorArgb);
            set => SquareColorArgb = value.ToArgb();
        }

        [JsonIgnore]
        public Color HorizontalSidesColor
        {
            get => Color.FromArgb(HorizontalSidesColorArgb);
            set => HorizontalSidesColorArgb = value.ToArgb();
        }

        [JsonIgnore]
        public Color VerticalSidesColor
        {
            get => Color.FromArgb(VerticalSidesColorArgb);
            set => VerticalSidesColorArgb = value.ToArgb();
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
                    var loaded = JsonSerializer.Deserialize<BreathingSettings>(json)
                                 ?? new BreathingSettings();
                    loaded.NormalizeLegacyColors();
                    return loaded;
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

        private void NormalizeLegacyColors()
        {
            if (HorizontalSidesColorArgb == 0)
            {
                HorizontalSidesColorArgb = SquareColorArgb;
            }

            if (VerticalSidesColorArgb == 0)
            {
                VerticalSidesColorArgb = SquareColorArgb;
            }
        }
    }
}
