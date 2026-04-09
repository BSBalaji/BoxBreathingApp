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
        private Button _btnSquareColor = null!;
        private Button _btnDotColor = null!;
        private Button _btnBgColor = null!;
        private Panel _previewSquare = null!;
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
            Size = new Size(340, 310);
            BackColor = Color.FromArgb(22, 22, 32);
            ForeColor = Color.FromArgb(200, 210, 230);
            Font = new Font("Segoe UI", 9.5f);

            // ── Timing ────────────────────────────────────────────────────
            var lblTiming = Label("Seconds per side (box-breathing step):", 18, 18);

            _nudSeconds = new NumericUpDown
            {
                Location = new Point(18, 42),
                Size = new Size(60, 26),
                Minimum = 1,
                Maximum = 60,
                BackColor = Color.FromArgb(36, 36, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = Font
            };

            var lblSec = Label("seconds  (4 = classic box breathing)", 84, 47);

            // ── Colors ────────────────────────────────────────────────────
            var lblColors = Label("Colors:", 18, 84);

            (_btnSquareColor, _previewSquare) = ColorRow("Square outline", 104);
            (_btnDotColor, _previewDot) = ColorRow("Moving dot", 140);
            (_btnBgColor, _previewBg) = ColorRow("Background", 176);

            // ── Buttons ───────────────────────────────────────────────────
            _btnApply = new Button
            {
                Text = "Apply",
                Location = new Point(130, 230),
                Size = new Size(80, 30),
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
                Location = new Point(222, 230),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(60, 60, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = Font,
                Cursor = Cursors.Hand
            };
            _btnReset.FlatAppearance.BorderSize = 0;
            _btnReset.Click += Reset;

            Controls.Add(lblTiming);
            Controls.Add(_nudSeconds);
            Controls.Add(lblSec);
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
            var lbl = Label(label + ":", 18, y + 4);
            Controls.Add(lbl);

            var preview = new Panel
            {
                Location = new Point(160, y),
                Size = new Size(24, 22),
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(preview);

            var btn = new Button
            {
                Text = "Pick…",
                Location = new Point(192, y - 1),
                Size = new Size(64, 24),
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
            _nudSeconds.Value = _settings.SecondsPerSide;
            _previewSquare.BackColor = _settings.SquareColor;
            _previewDot.BackColor = _settings.DotColor;
            _previewBg.BackColor = _settings.BackgroundColor;
        }

        private void Apply(object? sender, EventArgs e)
        {
            _settings.SecondsPerSide = (int)_nudSeconds.Value;
            _settings.SquareColor = _previewSquare.BackColor;
            _settings.DotColor = _previewDot.BackColor;
            _settings.BackgroundColor = _previewBg.BackColor;
            SettingsChanged?.Invoke();
        }

        private void Reset(object? sender, EventArgs e)
        {
            var def = new BreathingSettings();
            _nudSeconds.Value = def.SecondsPerSide;
            _previewSquare.BackColor = def.SquareColor;
            _previewDot.BackColor = def.DotColor;
            _previewBg.BackColor = def.BackgroundColor;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Subtle divider line
            e.Graphics.DrawLine(
                new System.Drawing.Pen(Color.FromArgb(50, 50, 70), 1),
                18, 218, Width - 18, 218);
        }
    }
}
