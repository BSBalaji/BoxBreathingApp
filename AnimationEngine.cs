using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace BoxBreathingTray
{
    /// <summary>
    /// Renders frames of the box-breathing animation into 32×32 tray icons
    /// at ~60 fps and delivers each new icon via a callback on the UI thread.
    /// </summary>
    public sealed class AnimationEngine : IDisposable
    {
        // Icon resolution — Windows tray icons are 32×32 (or 16×16 on older DPIs)
        private const int IconSize = 32;

        // Frames per second for smooth animation
        private const int Fps = 60;
        private static readonly int FrameMs = 1000 / Fps;

        private readonly Action<Icon> _iconCallback;
        private BreathingSettings _settings;

        private Thread? _thread;
        private volatile bool _running;
        private Icon? _lastIcon;

        // Track elapsed time so settings changes don't reset the animation
        private double _elapsedSeconds;
        private DateTime _lastTick;

        public AnimationEngine(BreathingSettings settings, Action<Icon> iconCallback)
        {
            _settings = settings;
            _iconCallback = iconCallback;
        }

        public void Start()
        {
            _running = true;
            _lastTick = DateTime.UtcNow;
            _thread = new Thread(Loop) { IsBackground = true, Name = "BoxBreathing" };
            _thread.Start();
        }

        public void Stop()
        {
            _running = false;
            _thread?.Join(500);
        }

        public void ApplySettings(BreathingSettings settings)
        {
            // Swap atomically enough for our purposes
            _settings = settings;
        }

        // ── Main animation loop ────────────────────────────────────────────

        private void Loop()
        {
            while (_running)
            {
                var now = DateTime.UtcNow;
                double delta = (now - _lastTick).TotalSeconds;
                _lastTick = now;
                _elapsedSeconds += delta;

                var settings = _settings; // local snapshot
                double cycleDuration = settings.SecondsPerSide * 4.0;
                double t = _elapsedSeconds % cycleDuration; // position in cycle [0, cycleDuration)

                // Normalise t → [0, 4) where each unit = one side
                double progress = (t / settings.SecondsPerSide) % 4.0;

                // Build icon and push to tray
                using var bmp = Render(progress, settings);
                var icon = BitmapToIcon(bmp);

                // Dispose previous icon after handing off the new one
                var oldIcon = _lastIcon;
                _lastIcon = icon;
                _iconCallback(icon);
                oldIcon?.Dispose();

                // Sleep for remainder of frame budget
                int elapsed = (int)(DateTime.UtcNow - now).TotalMilliseconds;
                int sleep = Math.Max(1, FrameMs - elapsed);
                Thread.Sleep(sleep);
            }
        }

        // ── Rendering ─────────────────────────────────────────────────────

        /// <summary>
        /// Renders one animation frame.
        /// progress ∈ [0, 4):
        ///   0–1  → top side (left → right)
        ///   1–2  → right side (top → bottom)
        ///   2–3  → bottom side (right → left)
        ///   3–4  → left side (bottom → top)
        /// </summary>
        private static Bitmap Render(double progress, BreathingSettings s)
        {
            var bmp = new Bitmap(IconSize, IconSize);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(s.BackgroundColor);

            // Square geometry (inset by 3px on each side for dot room)
            const int Margin = 6;
            const int Size = IconSize - Margin * 2;
            var rect = new Rectangle(Margin, Margin, Size, Size);

            // Draw square
            using var squarePen = new Pen(s.SquareColor, 3.5f);
            g.DrawRectangle(squarePen, rect);

            // Compute dot position
            var dotPos = GetDotPosition(progress, rect);

            // Draw dot with a soft glow
            float dotRadius = 2.8f;
            using var glowBrush = new SolidBrush(Color.FromArgb(60, s.DotColor));
            g.FillEllipse(glowBrush,
                dotPos.X - dotRadius * 2, dotPos.Y - dotRadius * 2,
                dotRadius * 4, dotRadius * 4);

            using var dotBrush = new SolidBrush(s.DotColor);
            g.FillEllipse(dotBrush,
                dotPos.X - dotRadius, dotPos.Y - dotRadius,
                dotRadius * 2, dotRadius * 2);

            return bmp;
        }

        /// <summary>Returns the dot centre in bitmap coordinates for the given progress.</summary>
        private static PointF GetDotPosition(double progress, Rectangle rect)
        {
            int x0 = rect.Left;
            int y0 = rect.Top;
            int x1 = rect.Right;
            int y1 = rect.Bottom;

            double side = progress % 4.0;
            double frac = side - Math.Floor(side); // 0..1 within this side

            return (int)Math.Floor(side) switch
            {
                0 => new PointF((float)(x0 + frac * rect.Width), y0),           // top: L→R
                1 => new PointF(x1, (float)(y0 + frac * rect.Height)),          // right: T→B
                2 => new PointF((float)(x1 - frac * rect.Width), y1),           // bottom: R→L
                3 => new PointF(x0, (float)(y1 - frac * rect.Height)),          // left: B→T
                _ => new PointF(x0, y0)
            };
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private static Icon BitmapToIcon(Bitmap bmp)
        {
            var hIcon = bmp.GetHicon();
            try
            {
                return Icon.FromHandle(hIcon);
            }
            finally
            {
                // Do NOT destroy hIcon here — Icon.FromHandle owns it; it is cleaned
                // up when the Icon is disposed. Calling DestroyIcon here would cause
                // a double-free crash.
            }
        }

        public void Dispose()
        {
            Stop();
            _lastIcon?.Dispose();
        }
    }
}
