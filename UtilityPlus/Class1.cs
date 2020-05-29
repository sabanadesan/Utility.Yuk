using System;
using System.Runtime.InteropServices;


namespace UtilityPlus
{
    public static class Class1
    {

        public const String flnm = "user32";

        [DllImport(flnm)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);

        public static void msg()
        {
            MessageBox(IntPtr.Zero, "Command-line message box", "Attention!", 0);
        }
    }
}
