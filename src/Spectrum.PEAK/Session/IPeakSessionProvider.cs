namespace Spectrum.PEAK.Session;

/// <summary>
/// Boundary between Spectrum-owned session logic and the PEAK-specific adapter.
/// Implement only after the game's actual lobby/player APIs and lifecycle have
/// been verified against the installed assemblies.
/// </summary>
internal interface IPeakSessionProvider
{
    PeakSessionSnapshot GetCurrentSnapshot();
    event Action<PeakSessionSnapshot>? SnapshotChanged;
}
