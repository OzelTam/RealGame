using System.Runtime.InteropServices;

namespace RealGame.Application
{
    internal static class Helpers
    {
        [DllImport("Kernel32.dll")]
        static extern nint GetConsoleWindow();

        [DllImport("User32.dll")]
        static extern bool ShowWindow(nint hWnd, int cmdShow);

        static nint hWnd = GetConsoleWindow();
        public static void HideConsole() => ShowWindow(hWnd, 0);
        public static void ShowConsole() => ShowWindow(hWnd, 1);
    }
}
