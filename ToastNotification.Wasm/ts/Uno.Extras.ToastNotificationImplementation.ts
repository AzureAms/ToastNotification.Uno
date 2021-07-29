declare const config: any;
declare const Module: any;

namespace Uno.Extras {
    export class ToastNotificationImplementation {
        private static ServiceWorkerName = "Notifications.ServiceWorker.js";
        private static _registered = false;
        private static _serviceWorker: ServiceWorkerRegistration;

        private static GetScriptsLocation() : string {
            var scriptLocation = "";

            var unoAppBase = config.uno_app_base;

            // Using some Uno Platform internal variables.
            if (typeof (unoAppBase) == "string") {
                scriptLocation += "./" + unoAppBase;
            }
            // A fallback using the Document.
            else {
                scriptLocation += Array
                    .from(document.scripts)
                    .map(script => script.baseURI + script.attributes.getNamedItem("src").value)
                    .find(name => name.endsWith("dotnet.js"))
                    .match(/([\S\s]*package_[a-zA-Z0-9]*)/g)[0];
            }

            scriptLocation += "/";

            return scriptLocation;
        }

        private static async RegisterServiceWorkerAsync(): Promise<ServiceWorkerRegistration> {
            var location = this.GetScriptsLocation();

            return await navigator.serviceWorker.register(location + this.ServiceWorkerName);
        }

        public static async Show(title: string, options: NotificationOptions): Promise<void> {
            if (!this._registered) {
                this._serviceWorker = await this.RegisterServiceWorkerAsync();
            }

            await this._serviceWorker.showNotification(title, options);
        }

        public static QueryPermissionAsync(): Promise<string> {

            return new Promise<string>(resolve => {
                function handlePermission(permission: NotificationPermission) {
                    resolve(permission.toString());
                }

                // Check Notification.requestPermission's syntax
                function checkNotificationPromise() {
                    try {
                        Notification.requestPermission().then();
                    }
                    catch (e) {
                        return false;
                    }
                    return true;
                }

                // Let's check if the browser supports notifications
                if (!('Notification' in window)) {
                    resolve('FeatureNotSupported');
                }
                else {
                    if (checkNotificationPromise()) {
                        Notification.requestPermission().then((permission) => {
                            handlePermission(permission);
                        })
                    }
                    else {
                        Notification.requestPermission(function (permission) {
                            handlePermission(permission);
                        });
                    }
                }
            });
        }

        public static GetButtonLimit(): Number {
            return Notification.maxActions;
        }
    }
}