// HotkeyDefinition.cs: one global hotkey to test.
// The three keys are the PRD FR-5 defaults: Ctrl+Alt+I, Ctrl+Alt+H, Alt+X.

namespace OverlayProbe;

internal sealed record HotkeyDefinition(int Id, string Label, uint Modifiers, uint VirtualKey)
{
    private const uint KeyH = 0x48;
    private const uint KeyI = 0x49;
    private const uint KeyX = 0x58;

    public static IReadOnlyList<HotkeyDefinition> WinIslandDefaults { get; } =
    [
        new(1, "Ctrl+Alt+I", NativeMethods.ModControl | NativeMethods.ModAlt, KeyI),
        new(2, "Ctrl+Alt+H", NativeMethods.ModControl | NativeMethods.ModAlt, KeyH),
        new(3, "Alt+X", NativeMethods.ModAlt, KeyX),
    ];
}
