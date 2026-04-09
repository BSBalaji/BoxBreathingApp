using System;
using System.Windows.Forms;

namespace BoxBreathingTray
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            // Run without a visible main form — tray icon is the UI
            Application.Run(new TrayApplicationContext());
        }
    }
}
