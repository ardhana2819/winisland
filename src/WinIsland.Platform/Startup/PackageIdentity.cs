// PackageIdentity.cs: tells whether WinIsland runs with its optional package identity
// (packaging/Install-WinIslandIdentity.ps1). Start with Windows needs to know (PRD FR-20): with
// identity, Windows gives WinIsland a private view of the registry's Run list that can differ from
// the real one (measured on Diva's PC, 10 Nov 2026), so WinIsland uses its startup task instead.

using System.Runtime.InteropServices;

namespace WinIsland.Platform.Startup;

public static partial class PackageIdentity
{
    // APPMODEL_ERROR_NO_PACKAGE: the process has no package identity.
    private const int NoPackage = 15700;

    /// <summary>True when this process runs with package identity.</summary>
    public static bool IsPresent()
    {
        int length = 0;
        return GetCurrentPackageFullName(ref length, IntPtr.Zero) != NoPackage;
    }

    // Asked with no buffer, it only reports whether there is a package name and how long it is.
    [LibraryImport("kernel32.dll")]
    private static partial int GetCurrentPackageFullName(ref int packageFullNameLength, IntPtr packageFullName);
}
