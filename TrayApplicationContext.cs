using System;
using System.Drawing;
using System.Windows.Forms;

namespace BoxBreathingTray
{
    /// <summary>
    /// Manages the system tray icon and overall application lifecycle.
    /// The animated icon IS the app — no main window is shown by default.
    /// </summary>
    public class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly BreathingSettings _settings;
        private readonly AnimationEngine _engine;
        private SettingsForm? _settingsForm;

        public TrayApplicationContext()
        {
            _settings = BreathingSettings.LoadOrDefault();

            _notifyIcon = new NotifyIcon
            {
                Visible = true,
                Text = "Box Breathing — 4-4-4-4",
                ContextMenuStrip = BuildContextMenu()
            };

            _engine = new AnimationEngine(_settings, icon =>
            {
                _notifyIcon.Icon = icon;
            });

            _engine.Start();
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();

            var openSettings = new ToolStripMenuItem("⚙  Settings…");
            openSettings.Click += OpenSettings;

            var separator = new ToolStripSeparator();

            var exitItem = new ToolStripMenuItem("✕  Exit");
            exitItem.Click += (_, __) =>
            {
                _engine.Stop();
                _notifyIcon.Visible = false;
                Application.Exit();
            };

            menu.Items.Add(openSettings);
            menu.Items.Add(separator);
            menu.Items.Add(exitItem);
            return menu;
        }

        private void OpenSettings(object? sender, EventArgs e)
        {
            if (_settingsForm == null || _settingsForm.IsDisposed)
            {
                _settingsForm = new SettingsForm(_settings);
                _settingsForm.SettingsChanged += () =>
                {
                    _settings.Save();
                    _engine.ApplySettings(_settings);
                };
                _settingsForm.Show();
            }
            else
            {
                _settingsForm.BringToFront();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _engine.Stop();
                _notifyIcon.Dispose();
                _settingsForm?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
