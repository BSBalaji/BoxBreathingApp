using System;
using System.Drawing;
using System.Windows.Forms;

namespace BoxBreathingTray
{
    /// <summary>
    /// A compact settings window that lets the user tweak colors and timing.
    /// Fires SettingsChanged when the user applies changes.
    /// </summary>
    public class SettingsForm : Form
    {
        public event Action? SettingsChanged;

        private readonly BreathingSettings _settings;

        // Controls
        private NumericUpDown _nudSeconds = null!;
        private CheckBox _chkEnableReminder = null!;
        private NumericUpDown _nudReminderMinutes = null!;
        private Button _btnHorizontalColor = null!;
        private Button _btnVerticalColor = null!;
        private Button _btnDotColor = null!;
        private Button _btnBgColor = null!;
        private Panel _previewHorizontal = null!;
        private Panel _previewVertical = null!;
        private Panel _previewDot = null!;
        private Panel _previewBg = null!;
        private Button _btnApply = null!;
        private Button _btnReset = null!;

        public SettingsForm(BreathingSettings settings)
        {
            _settings = settings;
            BuildUI();
            LoadValues();
        }

        private void BuildUI()
        {
            Text = "Box Breathing — Settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(460, 470);
            BackColor = Color.FromArgb(22, 22, 32);
            ForeColor = Color.FromArgb(200, 210, 230);
            Font = new Font("Segoe UI", 9.5f);

            // ── Timing ────────────────────────────────────────────────────
            var lblTiming = Label("Dot movement time per side:", 24, 22);

            _nudSeconds = new NumericUpDown
            {
                Location = new Point(24, 52),
                Size = new Size(90, 30),
                Minimum = 0.5m,
                Maximum = 60,
                DecimalPlaces = 1,
                Increment = 0.1m,
                BackColor = Color.FromArgb(36, 36, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = Font
            };

            var lblSec = Label("seconds  (4.0 = classic box breathing)", 126, 58);

            var lblReminder = Label("Eye-break reminder:", 24, 108);
            _chkEnableReminder = new CheckBox
            {
                Text = "Enable tray notification reminder",
                Location = new Point(24, 138),
                Size = new Size(320, 28),
                ForeColor = Color.FromArgb(180, 190, 210),
                BackColor = Color.Transparent,
                Font = Font
            };

            _nudReminderMinutes = new NumericUpDown
            {
                Location = new Point(24, 174),
                Size = new Size(90, 30),
                Minimum = 1,
                Maximum = 240,
                DecimalPlaces = 0,
                Increment = 1,
                BackColor = Color.FromArgb(36, 36, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = Font
            };

            var lblReminderUnit = Label("minutes between reminders", 126, 182);

            // ── Colors ────────────────────────────────────────────────────
            var lblColors = Label("Colors:", 24, 230);

            (_btnHorizontalColor, _previewHorizontal) = ColorRow("Top + Bottom sides", 262);
            (_btnVerticalColor, _previewVertical) = ColorRow("Left + Right sides", 302);
            (_btnDotColor, _previewDot) = ColorRow("Moving dot", 342);
            (_btnBgColor, _previewBg) = ColorRow("Background", 382);

            // ── Buttons ───────────────────────────────────────────────────
            _btnApply = new Button
            {
                Text = "Apply",
                Location = new Point(262, 424),
                Size = new Size(84, 32),
                BackColor = Color.FromArgb(60, 100, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = Font,
                Cursor = Cursors.Hand
            };
            _btnApply.FlatAppearance.BorderSize = 0;
            _btnApply.Click += Apply;

            _btnReset = new Button
            {
                Text = "Reset",
                Location = new Point(356, 424),
                Size = new Size(84, 32),
                BackColor = Color.FromArgb(60, 60, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = Font,
                Cursor = Cursors.Hand
            };
            _btnReset.FlatAppearance.BorderSize = 0;
            _btnReset.Click += Reset;
            _chkEnableReminder.CheckedChanged += (_, __) =>
            {
                _nudReminderMinutes.Enabled = _chkEnableReminder.Checked;
            };

            Controls.Add(lblTiming);
            Controls.Add(_nudSeconds);
            Controls.Add(lblSec);
            Controls.Add(lblReminder);
            Controls.Add(_chkEnableReminder);
            Controls.Add(_nudReminderMinutes);
            Controls.Add(lblReminderUnit);
            Controls.Add(lblColors);
            Controls.Add(_btnApply);
            Controls.Add(_btnReset);
        }

        private Label Label(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(180, 190, 210),
                BackColor = Color.Transparent,
                Font = Font
            };
        }

        /// <summary>Creates a color picker row with a button and a small color swatch.</summary>
        private (Button btn, Panel preview) ColorRow(string label, int y)
        {
            var lbl = Label(label + ":", 24, y + 5);
            Controls.Add(lbl);

            var preview = new Panel
            {
                Location = new Point(300, y + 3),
                Size = new Size(28, 24),
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(preview);

            var btn = new Button
            {
                Text = "Pick…",
                Location = new Point(338, y + 1),
                Size = new Size(72, 28),
                BackColor = Color.FromArgb(46, 46, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (_, __) =>
            {
                using var dlg = new ColorDialog { Color = preview.BackColor, FullOpen = true };
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    preview.BackColor = dlg.Color;
            };
            Controls.Add(btn);
            return (btn, preview);
        }

        private void LoadValues()
        {
            _nudSeconds.Value = (decimal)_settings.SecondsPerSide;
            _chkEnableReminder.Checked = _settings.EnableEyeBreakReminder;
            _nudReminderMinutes.Value = _settings.EyeBreakReminderMinutes;
            _nudReminderMinutes.Enabled = _chkEnableReminder.Checked;
            _previewHorizontal.BackColor = _settings.HorizontalSidesColor;
            _previewVertical.BackColor = _settings.VerticalSidesColor;
            _previewDot.BackColor = _settings.DotColor;
            _previewBg.BackColor = _settings.BackgroundColor;
        }

        private void Apply(object? sender, EventArgs e)
        {
            _settings.SecondsPerSide = (double)_nudSeconds.Value;
            _settings.EnableEyeBreakReminder = _chkEnableReminder.Checked;
            _settings.EyeBreakReminderMinutes = (int)_nudReminderMinutes.Value;
            _settings.HorizontalSidesColor = _previewHorizontal.BackColor;
            _settings.VerticalSidesColor = _previewVertical.BackColor;
            _settings.DotColor = _previewDot.BackColor;
            _settings.BackgroundColor = _previewBg.BackColor;
            SettingsChanged?.Invoke();
        }

        private void Reset(object? sender, EventArgs e)
        {
            var def = new BreathingSettings();
            _nudSeconds.Value = (decimal)def.SecondsPerSide;
            _chkEnableReminder.Checked = def.EnableEyeBreakReminder;
            _nudReminderMinutes.Value = def.EyeBreakReminderMinutes;
            _previewHorizontal.BackColor = def.HorizontalSidesColor;
            _previewVertical.BackColor = def.VerticalSidesColor;
            _previewDot.BackColor = def.DotColor;
            _previewBg.BackColor = def.BackgroundColor;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Subtle divider line
            e.Graphics.DrawLine(
                new System.Drawing.Pen(Color.FromArgb(50, 50, 70), 1),
                24, 414, ClientSize.Width - 24, 414);
        }
    }
}
