using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tmds.DBus;

// https://developer.gnome.org/notification-spec/#protocol
[assembly: InternalsVisibleTo(Connection.DynamicAssemblyName)]
namespace Notifications.DBus
{
    [DBusInterface("org.freedesktop.Notifications")]
    internal interface INotifications : IDBusObject
    {
        /// <summary>
        /// Sends a notification to the notification server.
        /// </summary>
        /// <param name="app_name">The optional name of the application sending the notification. Can be blank.</param>
        /// <param name="replaces_id">The optional notification ID that this notification replaces. The server must atomically (ie with no flicker or other visual cues) replace the given notification with this one. This allows clients to effectively modify the notification while it's active. A value of value of 0 means that this notification won't replace any existing notifications.</param>
        /// <param name="app_icon">The optional program icon of the calling application. See Icons and Images. Can be an empty string, indicating no icon.</param>
        /// <param name="summary">The summary text briefly describing the notification.</param>
        /// <param name="body">The optional detailed body text. Can be empty.</param>
        /// <param name="actions">Actions are sent over as a list of pairs. Each even element in the list (starting at index 0) represents the identifier for the action. Each odd element in the list is the localized string that will be displayed to the user.</param>
        /// <param name="hints">	Optional hints that can be passed to the server from the client program. Although clients and servers should never assume each other supports any specific hints, they can be used to pass along information, such as the process PID or window ID, that the server may be able to make use of. See Hints. Can be empty.</param>
        /// <param name="expire_timeout">The timeout time in milliseconds since the display of the notification at which the notification should automatically close. If -1, the notification's expiration time is dependent on the notification server's settings, and may vary for the type of notification.If 0, never expire.</param>
        /// <returns>A UINT32 that represent the notification</returns>
        Task<uint> NotifyAsync(string app_name, uint replaces_id, string app_icon, string summary, string body, string[] actions, IDictionary<string, object> hints, int expire_timeout);
        /// <summary>
        /// Causes a notification to be forcefully closed and removed from the user's view. It can be used, for example, in the event that what the notification pertains to is no longer relevant, or to cancel a notification with no expiration time.
        /// </summary>
        /// <param name="id">The id of the notification</param>
        Task CloseNotificationAsync(uint id);
        /// <summary>
        /// Get capabilites of the notification service.
        /// </summary>
        /// <returns>An array of strings, each string describes an optional capability implemented by the server.</returns>
        Task<string[]> GetCapabilitiesAsync();
        /// <summary>
        /// This message returns the information on the server. Specifically, the server name, vendor, and version number.
        /// </summary>
        /// <returns>Information on the server.</returns>
        Task<(string name, string vendor, string version, string spec_version)> GetServerInformationAsync();
        /// <summary>
        /// Registers to the NotificationClosed signal
        /// </summary>
        /// <param name="handler">The handler</param>
        /// <param name="onError">The callback if error happens</param>
        /// <returns>An IDisposable, used for unregistration.</returns>
        Task<IDisposable> WatchNotificationClosedAsync(Action<(uint id, uint reason)> handler, Action<Exception> onError = null);
        /// <summary>
        /// Registers to the ActionInvoked signal
        /// </summary>
        /// <param name="handler">The handler</param>
        /// <param name="onError">The callback if error happens</param>
        /// <returns>An IDisposable, used for unregistration.</returns>
        Task<IDisposable> WatchActionInvokedAsync(Action<(uint id, string action_key)> handler, Action<Exception> onError = null);
    }
}