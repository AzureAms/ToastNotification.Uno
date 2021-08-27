using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Notification.FrameworkDependent
{
    /// <summary>
    /// Provide wrappers for Icon-related Win32 Functions
    /// </summary>
    internal static class IconInterop
    {
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

        const uint WM_GETICON = 0x007f;
        private static readonly IntPtr ICON_SMALL2 = new IntPtr(2);
        const int GCL_HICON = -14;

        /// <summary>
        /// 64 bit version maybe loses significant 64-bit specific information
        /// </summary>
        static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return new IntPtr((long)GetClassLong32(hWnd, nIndex));
            }
            else
            {
                return GetClassLong64(hWnd, nIndex);
            }
        }

        public static IntPtr GetSmallWindowIcon(IntPtr hWnd)
        {
            try
            {
                IntPtr hIcon = default(IntPtr);

                hIcon = SendMessage(hWnd, WM_GETICON, ICON_SMALL2, IntPtr.Zero);

                if (hIcon == IntPtr.Zero)
                {
                    hIcon = GetClassLongPtr(hWnd, GCL_HICON);
                }

                if (hIcon == IntPtr.Zero)
                {
                    hIcon = LoadIcon(IntPtr.Zero, (IntPtr)0x7F00/*IDI_APPLICATION*/);
                }

                return hIcon;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        public static ImageSource GetSmallWindowIcon()
        {
            try
            {
                var window = Application.Current.MainWindow;
                var wih = new WindowInteropHelper(window);
                var hWnd = wih.Handle;

                return Imaging.CreateBitmapSourceFromHIcon(
                    GetSmallWindowIcon(hWnd),
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failed to get Application Icon: {e}");
                Debug.WriteLine("Using default icon.");

                return Imaging.CreateBitmapSourceFromHIcon(
                    LoadIcon(IntPtr.Zero, (IntPtr)0x7F00),
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
        }
    }
}
