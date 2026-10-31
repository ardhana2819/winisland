// NotificationArrival.cs: a new Windows notification plus the app's logo, when Windows has one
// (DESIGN_SPEC section 4: use the app logo from the notification, fallback is a letter circle).

using WinIsland.Core.Notifications;

namespace WinIsland.Platform.Notifications;

public sealed record NotificationArrival(RawNotification Notification, byte[]? AppLogo);
