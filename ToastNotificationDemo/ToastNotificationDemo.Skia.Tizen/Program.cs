using Tizen.Applications;
using Uno.UI.Runtime.Skia;

namespace ToastNotificationDemo.Skia.Tizen
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new TizenHost(() => new ToastNotificationDemo.App(), args);
            host.Run();
        }
    }
}
