using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Notification.Natives
{
    internal static class MessageWindow
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate IntPtr WNDPROC(IntPtr hWnd, uint uMsg, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U2)]
        static extern short RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll")]
        static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, UIntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WNDCLASSEX
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int style;
            public WNDPROC lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr CreateWindowEx(
           uint dwExStyle,
           string lpClassName,
           string lpWindowName,
           uint dwStyle,
           int x,
           int y,
           int nWidth,
           int nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam);

        static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);
        // Keeps a reference so that the Garbage Collector won't mess up.
        static readonly WNDPROC _mainWndProc = WndProc;

        private static Dictionary<IntPtr, WNDPROC> _wndProcs = new Dictionary<IntPtr, WNDPROC>();

        static MessageWindow()
        {
            var wndClass = new WNDCLASSEX
            {
                cbSize = Marshal.SizeOf(typeof(WNDCLASSEX)),
                lpfnWndProc = _mainWndProc,
                hInstance = Process.GetCurrentProcess().Handle,
                lpszClassName = typeof(MessageWindow).FullName
            };

            var _regResult = RegisterClassEx(ref wndClass);

            if (_regResult == 0)
            {
                Debug.WriteLine($"Failed to register MessageWindow class: {Marshal.GetLastWin32Error()}");
            }

        }

        public static IntPtr Create(WNDPROC handler = null)
        {
            var hWnd = CreateWindowEx(
                0,
                typeof(MessageWindow).FullName, // window class name
                null, // window caption
                0, // window style
                0, // initial x position
                0, // initial y position
                0, // initial x size
                0, // initial y size
                HWND_MESSAGE, // parent window handle
                IntPtr.Zero, // window menu handle
                IntPtr.Zero, // program instance handle
                IntPtr.Zero); // creation parameters

            if (handler != null)
            {
                _wndProcs.Add(hWnd, handler);
            }

            return hWnd;
        }

        public static void Destroy(IntPtr hWnd)
        {
            DestroyWindow(hWnd);
            _wndProcs.Remove(hWnd);
        }

        private static IntPtr WndProc(IntPtr hWnd, uint uMsg, UIntPtr wParam, IntPtr lParam)
        {
            if (_wndProcs.TryGetValue(hWnd, out WNDPROC handler))
            {
                return handler(hWnd, uMsg, wParam, lParam);
            }

            return DefWindowProc(hWnd, uMsg, wParam, lParam);
        }
    }
}
