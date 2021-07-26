using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Notification.FrameworkDependent
{
    /// <summary>
    /// Provides wrappers for Screen position related Win32 functions
    /// </summary>
    internal static class ScreenInterop
    {
        #region Unmanaged Functions
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref RECT pvParam, uint fWinIni);

        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetCursorPos(out POINT lpPoint);
        #endregion

        #region Constants
        const uint SPI_GETWORKAREA = 0x0030;
        #endregion

        #region Structs
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            int left;
            int top;
            int right;
            int bottom;

            public static implicit operator Rect(RECT r)
            {
                Debug.WriteLine(r.left);
                Debug.WriteLine(r.top);
                Debug.WriteLine(r.right);
                Debug.WriteLine(r.bottom);

                var factor = GetScaleFactor();

                Debug.WriteLine(factor);

                return new Rect
                {
                    X = r.left / factor,
                    Y = r.top / factor,
                    Width = (r.right - r.left) / factor,
                    Height = (r.bottom - r.top) / factor
                };
            }
        }

        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }
        #endregion

        public static Rect GetWorkArea()
        {
            var rect = new RECT();

            if (!SystemParametersInfo(SPI_GETWORKAREA, 0, ref rect, 0))
            {
                int error = Marshal.GetLastWin32Error();
                throw new Win32Exception(error);
            }

            return rect;
        }

        private static float GetScaleFactor()
        {
            IntPtr desktopWnd = IntPtr.Zero;
            IntPtr dc = GetDC(desktopWnd);
            var dpi = 100f;
            const int LOGPIXELSX = 88;
            try
            {
                dpi = GetDeviceCaps(dc, LOGPIXELSX);
            }
            finally
            {
                ReleaseDC(desktopWnd, dc);
            }
            return dpi / 96f;
        }

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            
            if (!GetCursorPos(out lpPoint))
            {
                int error = Marshal.GetLastWin32Error();
                throw new Win32Exception(error);
            }

            return lpPoint;
        }
    }
}
