using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Natives
{
    internal class NotificationManagerWindow : IDisposable
    {
        private bool disposedValue;

        public IntPtr Handle { get; private set; }
        public uint CallbackMessgae { get; private set; }

        public NotificationManagerWindow(uint callbackMessage)
        {
            CallbackMessgae = callbackMessage;
            Handle = MessageWindow.Create(WndProc);
        }

        public event EventHandler BalloonShow;
        public event EventHandler BalloonHide;
        public event EventHandler BallonClicked;

        private IntPtr WndProc(IntPtr hWnd, uint uMsg, UIntPtr wParam, IntPtr lParam)
        {
            if (uMsg != CallbackMessgae)
            {
                return MessageWindow.DefWindowProc(hWnd, uMsg, wParam, lParam);
            }

            switch ((NotificationIconNotify)(lParam.ToInt64() & 0xFFFFFFFF))
            {
                case NotificationIconNotify.BalloonShow:
                    BalloonShow?.Invoke(this, EventArgs.Empty);
                    return IntPtr.Zero;
                case NotificationIconNotify.BalloonHide:
                case NotificationIconNotify.BalloonTimeout:
                    BalloonHide?.Invoke(this, EventArgs.Empty);
                    return IntPtr.Zero;
                case NotificationIconNotify.BalloonClick:
                    BallonClicked?.Invoke(this, EventArgs.Empty);
                    return IntPtr.Zero;
                default:
                    return MessageWindow.DefWindowProc(hWnd, uMsg, wParam, lParam);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    BalloonShow.CleanEvent();
                    BalloonHide.CleanEvent();
                    BallonClicked.CleanEvent();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                MessageWindow.Destroy(Handle);
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~NotificationManagerWindow()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
