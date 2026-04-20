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
        private readonly Timer _eyeBreakReminderTimer;
        private SettingsForm? _settingsForm;
        private ToolStripMenuItem? _pauseResumeItem;

        public TrayApplicationContext()
        {
            _settings = BreathingSettings.LoadOrDefault();

            _notifyIcon = new NotifyIcon
            {
                Visible = true,
                Text = BuildTooltipText(false),
                ContextMenuStrip = BuildContextMenu()
            };
            _notifyIcon.MouseClick += OnNotifyIconMouseClick;

            _engine = new AnimationEngine(_settings, icon =>
            {
                _notifyIcon.Icon = icon;
            });

            _eyeBreakReminderTimer = new Timer();
            _eyeBreakReminderTimer.Tick += OnEyeBreakReminderTick;
            ApplyReminderSettings();

            _engine.Start();
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();

            var openSettings = new ToolStripMenuItem("⚙  Settings…");
            openSettings.Click += OpenSettings;

            _pauseResumeItem = new ToolStripMenuItem("⏸  Pause");
            _pauseResumeItem.Click += (_, __) => TogglePause();

            var separator = new ToolStripSeparator();

            var exitItem = new ToolStripMenuItem("✕  Exit");
            exitItem.Click += (_, __) =>
            {
                _engine.Stop();
                _notifyIcon.Visible = false;
                Application.Exit();
            };

            menu.Items.Add(openSettings);
            menu.Items.Add(_pauseResumeItem);
            menu.Items.Add(separator);
            menu.Items.Add(exitItem);
            return menu;
        }

        private string BuildTooltipText(bool paused)
        {
            string status = paused ? "Paused" : "Running";
            return $"Box Breathing ({status}) — {_settings.SecondsPerSide:0.0}s/side";
        }

        private void TogglePause()
        {
            bool isPaused = _engine.TogglePause();
            if (_pauseResumeItem != null)
            {
                _pauseResumeItem.Text = isPaused ? "▶  Resume" : "⏸  Pause";
            }
            _notifyIcon.Text = BuildTooltipText(isPaused);
        }

        private void OnNotifyIconMouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TogglePause();
            }
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
                    ApplyReminderSettings();
                    _notifyIcon.Text = BuildTooltipText(_engine.IsPaused);
                };
                _settingsForm.Show();
            }
            else
            {
                _settingsForm.BringToFront();
            }
        }

        private void ApplyReminderSettings()
        {
            _eyeBreakReminderTimer.Stop();

            if (!_settings.EnableEyeBreakReminder)
            {
                return;
            }

            int maxMinutes = int.MaxValue / (60 * 1000);
            int intervalMinutes = Math.Clamp(_settings.EyeBreakReminderMinutes, 1, maxMinutes);
            int intervalMs = intervalMinutes * 60 * 1000;
            _eyeBreakReminderTimer.Interval = intervalMs;
            _eyeBreakReminderTimer.Start();
        }

        private void OnEyeBreakReminderTick(object? sender, EventArgs e)
        {
            _notifyIcon.BalloonTipTitle = "Eye Break Reminder";
            _notifyIcon.BalloonTipText = "Look at something far away and blink for a moment.";
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            _notifyIcon.ShowBalloonTip(4000);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _engine.Stop();
                _eyeBreakReminderTimer.Stop();
                _eyeBreakReminderTimer.Dispose();
                _notifyIcon.Dispose();
                _settingsForm?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
