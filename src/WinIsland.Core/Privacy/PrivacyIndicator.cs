// PrivacyIndicator.cs: which dot the idle island shows while an app uses the camera or the
// microphone, like the iPhone (PRD FR-40, DESIGN_SPEC section 12). Camera green, microphone
// orange; with both in use the camera wins, because it says more.
// Windows keeps a record per app of when it last started and stopped using each one; an app uses
// it now when it has started and not stopped since (Stop is 0).

namespace WinIsland.Core.Privacy;

public enum PrivacyIndicator
{
    None,
    Microphone,
    Camera,
}

public static class PrivacyIndicators
{
    public static PrivacyIndicator For(bool cameraInUse, bool microphoneInUse)
    {
        if (cameraInUse)
        {
            return PrivacyIndicator.Camera;
        }

        return microphoneInUse ? PrivacyIndicator.Microphone : PrivacyIndicator.None;
    }

    /// <summary>True when one app's record says it uses the device right now.</summary>
    public static bool IsInUse(long lastUsedStart, long lastUsedStop)
    {
        return lastUsedStart > 0 && lastUsedStop == 0;
    }
}
