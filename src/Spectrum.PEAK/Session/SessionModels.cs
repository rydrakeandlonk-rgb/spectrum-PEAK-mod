namespace Spectrum.PEAK.Session;

/// <summary>
/// Game-neutral snapshot shape for the future PEAK session adapter.
/// These are Spectrum-owned models, not claims about PEAK's internal types.
/// </summary>
internal sealed class PeakSessionSnapshot
{
    public bool IsInLobby { get; set; }
    public bool IsLocalHost { get; set; }
    public string? SessionId { get; set; }
    public IReadOnlyList<PeakPlayerSnapshot> Players { get; set; } = Array.Empty<PeakPlayerSnapshot>();
}

internal sealed class PeakPlayerSnapshot
{
    public string? NetworkIdentity { get; set; }
    public string? DisplayName { get; set; }
    public bool IsLocalPlayer { get; set; }
}
