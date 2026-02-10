/*
 Author: wangchaozhi
 Date: 2026/02/10
*/

using Avalonia.Controls;

namespace AvaloniaApplication1.Service
{
    public static class WindowActivationService
    {
        public static void Activate(Window? window)
        {
            if (window is null)
                return;

            if (!window.IsVisible)
                window.Show();

            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;

            window.Activate();

            // Windows 上 Activate() 有时不会可靠置顶，这里用 Topmost 翻转做兜底
            var oldTopmost = window.Topmost;
            window.Topmost = true;
            window.Topmost = oldTopmost;

            window.Focus();
        }
    }
}

