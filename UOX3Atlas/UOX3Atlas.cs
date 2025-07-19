using System;
using System.Windows.Forms;

namespace UOX3Atlas
{
    internal static class UOX3Atlas
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
