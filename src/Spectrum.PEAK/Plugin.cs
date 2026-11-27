using BepInEx;
using BepInEx.Logging;
using Spectrum.PEAK.Configuration;
using Spectrum.PEAK.Diagnostics;

namespace Spectrum.PEAK;

/// <summary>
/// Entry point for the Spectrum PEAK BepInEx plugin.
/// PEAK-specific hooks are intentionally withheld until the installed game
/// assemblies have been inspected and the relevant APIs verified.
/// </summary>
[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "rydrake.spectrum.peak";
    public const string PluginName = "Spectrum PEAK";
    public const string PluginVersion = "0.1.0";

    internal static ManualLogSource Log { get; private set; } = null!;
    internal static PluginSettings Settings { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;
        Settings = new PluginSettings(Config);

        Log.LogInfo($"{PluginName} {PluginVersion} loaded.");
        PluginDiagnostics.LogStartup(Log, Settings);
    }

    private void OnDestroy()
    {
        Log.LogInfo($"{PluginName} shutting down.");
    }
}
