using BepInEx;
using BepInEx.Logging;
using Spectrum.PEAK.Configuration;
using Spectrum.PEAK.Connection;
using Spectrum.PEAK.Diagnostics;

namespace Spectrum.PEAK;

/// <summary>
/// Entry point for the Spectrum PEAK BepInEx plugin.
/// The initial integration establishes the authenticated Spectrum connection;
/// PEAK gameplay APIs remain isolated behind a later verified adapter.
/// </summary>
[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "rydrake.spectrum.peak";
    public const string PluginName = "Spectrum PEAK";
    public const string PluginVersion = "0.2.0";

    internal static ManualLogSource Log { get; private set; } = null!;
    internal static PluginSettings Settings { get; private set; } = null!;

    private PeakConnectionClient? _connection;

    private void Awake()
    {
        Log = Logger;
        Settings = new PluginSettings(Config);

        Log.LogInfo($"{PluginName} {PluginVersion} loaded.");
        PluginDiagnostics.LogStartup(Log, Settings);

        _connection = new PeakConnectionClient(
            Settings.EnableIntegration,
            Settings.HostToken,
            Settings.BroadcasterUserId);
        _connection.Start();
    }

    private void OnDestroy()
    {
        _connection?.Dispose();
        Log.LogInfo($"{PluginName} shutting down.");
    }
}
