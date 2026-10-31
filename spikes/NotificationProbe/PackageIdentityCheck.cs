// PackageIdentityCheck.cs: tells us whether this process is running with package identity.
// Spike S1 needs this to prove that the identity package from packaging/ actually took effect.

using Windows.ApplicationModel;

namespace NotificationProbe;

internal static class PackageIdentityCheck
{
    public static string Describe()
    {
        try
        {
            // Package.Current throws when the process has no package identity.
            return $"YES ({Package.Current.Id.FullName})";
        }
        catch (InvalidOperationException)
        {
            return "NO (running unpackaged)";
        }
        catch (Exception error)
        {
            return $"NO ({error.GetType().Name}, HRESULT 0x{error.HResult:X8})";
        }
    }
}
