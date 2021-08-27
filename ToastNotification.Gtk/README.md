# ToastNotification.Uno.Gtk  

Special care must be taken when using toast notifications on Gtk, in order for the toast to show correctly:  
-   ~~You must create a new [`GLib.Application`](https://developer.gnome.org/gio/stable/GApplication.html#g-application-send-notification) instance, passing the valid application id.~~  
-   ~~You must create a valid `.desktop` file for your application. For more details see [this](https://specifications.freedesktop.org/desktop-entry-spec/latest/) specifiation.~~
-   Icons for Gtk notifications should be small and a perfect square. Normal pictures that may look well on Windows and Android as a icon may not be satisfactory on Gtk.  

~~For more details study [this documentation](https://developer.gnome.org/GNotification/).~~

Toast Notifications are now implemented using [DBus](https://developer.gnome.org/notification-spec/).
This removes any `.desktop` file ore `GLib.Application` requirements.
The implementation has been well tested on Ubuntu 21.04, but may have glitches
on other systems. Feel free to open an issue in this case.