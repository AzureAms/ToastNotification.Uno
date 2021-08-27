/* eslint no-console: ["error", { allow: ["warn", "error"] }] */

//@ts-ignore
_self = self;
declare var _self: any;

namespace Notifications.ServiceWorker {
    export class NotificationWorker {
        private static _resolves: Record<number, any> = { };
        private static _num = 0;
        // This should be adequate for most responsive modern browsers.
        private static _timeout = 2048;
        public static Run() {
            if (self.document) {
                return;
            }
            self.onmessage = this.HandleMessage.bind(this);

            self.addEventListener("install", function (event: any) {
                event.waitUntil(_self.skipWaiting()); // Activate worker immediately
            });

            self.addEventListener("activate", function (event: any) {
                event.waitUntil(_self.clients.claim()); // Become available to all pages
            });

            // @ts-ignore
            self.onnotificationclick = (event: any) => {
                event.notification.close();

                var eventId: string;

                if (event.action) {
                    eventId = event.action;
                }
                else {
                    eventId = event.notification.tag;
                }

                var eventData = JSON.parse(eventId);

                var type = eventData.type;
                var guid = eventData.guid;

                switch (type) {
                    case "dismiss":
                        break;
                    case "background":
                        throw new DOMException("Uno Platform does not support background tasks", "System.NotImplementedException");
                    case "protocol":
                        var url = eventData.argument;
                        _self.clients.openWindow(url);
                        break;
                    case "foreground":
                        var mainPageUrl = (new URL("..", _self.registration.scope)).href;

                        // This looks to see if the current is already open and
                        // focuses if it is
                        event.waitUntil(new Promise<void>(resolve => {
                            _self.clients.matchAll({
                                includeUncontrolled: true,
                                type: "window"
                            }).then((clientWindows: any[]) => {
                                clientWindows.reduce((p, client) => {
                                    return p.then((isSuccessful: boolean) => {
                                        if (isSuccessful) {
                                            return Promise.resolve(true);
                                        }
                                        return new Promise<boolean>(resolveChild => {
                                            if (client.url !== mainPageUrl) {
                                                resolveChild(false);
                                            }
                                            if (!("focus" in client)) {
                                                resolveChild(false);
                                            }
                                            new Promise<boolean>(resolveCommunication => {
                                                var tempChannel = new MessageChannel();
                                                tempChannel.port1.onmessage = (ev) => {
                                                    resolveCommunication(ev.data.payload);
                                                };
                                                client.postMessage({ op: "CHECK_GUID", payload: guid }, [tempChannel.port2]);
                                                setTimeout(() => resolveCommunication(false), 1000);
                                            }).then((isSuccessful) => {
                                                if (!isSuccessful) {
                                                    resolveChild(false);
                                                }
                                                else {
                                                    client.focus().then((_: any) => {
                                                        this.PumpMessage(client, "notificationclick", eventData.argument);
                                                        resolveChild(true);
                                                    });
                                                }
                                            });
                                        });
                                    });
                                }, Promise.resolve(false)).then((isSuccessful: boolean) => {
                                    if (isSuccessful) {
                                        resolve();
                                    }
                                    else if (_self.clients.openWindow) {
                                        _self.clients.openWindow(mainPageUrl).then((client: any) => {
                                            this.PumpMessage(client, "notificationclick", eventData.argument);
                                        });
                                    }
                                });
                            });
                        }));
                        break;
                }
            };
        }

        private static async PumpMessage(client: any, eventOp: string, eventArg: any) : Promise<void> {
            var num = this._num++;
            for (var i = 0; i < 128; ++i) {
                var doWork = new Promise<boolean>(resolve => {
                    this._resolves[num] = resolve;
                    var tempChannel = new MessageChannel();
                    tempChannel.port1.onmessage = this.HandleMessage.bind(this);
                    client.postMessage({ op: eventOp, payload: { arg: eventArg, seq: num } }, [tempChannel.port2]);
                });
                var delay = new Promise<boolean>(resolve => {
                    this.Delay(this._timeout).then(() => resolve(false));
                });
                if (await Promise.race([doWork, delay])) {
                    break;
                }
                console.warn("Failed to send notification. Retrying... " + i);
            }
            delete this._resolves[num];
        }

        private static Delay(milliseconds: number): Promise<void> {
            return new Promise<void>(resolve => {
                setTimeout(() => resolve(), milliseconds);
            });
        }

        private static HandleMessage(event: MessageEvent) {
            if (event.data.op) {
                switch (event.data.op) {
                    case "SET_PORT":
                        var guid = event.data.payload.guid;
                        console.warn("WTF why are you sending ports?");
                        //this._ports[guid] = event.ports[0];
                        //this._ports[guid].onmessage = this.HandleMessage.bind(this);
                        //this._ports[guid].postMessage({ op: "ack" });
                        break;
                    case "REMOVE_PORT":
                        //var guid = event.data.payload.guid;
                        //delete this._ports[guid];
                        //break;
                        console.warn("Do I have any ports to remove?");
                        break;
                    case "SHOW_NOTIFICATION":
                        // @ts-ignore
                        self.registration.showNotification(event.data.payload.title, event.data.payload.options);
                        break;
                    case "ACK":
                        var num = event.data.payload;
                        if (num in this._resolves) {
                            this._resolves[num](true);
                        }
                        break;
                }
            }
        }
    }
}

Notifications.ServiceWorker.NotificationWorker.Run();