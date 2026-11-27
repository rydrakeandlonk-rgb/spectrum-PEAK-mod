using BepInEx.Logging;
using Spectrum.PEAK.Configuration;

namespace Spectrum.PEAK.Diagnostics;

/// <summary>Startup diagnostics that do not inspect or mutate PEAK gameplay state.</summary>
internal static class PluginDiagnostics
{
    public static void LogStartup(ManualLogSource logger, PluginSettings settings)
    {
        logger.LogInfo("Runtime: BepInEx plugin bootstrap is active.");
        logger.LogInfo("Spectrum connection: " + (settings.EnableIntegration.Value ? "enabled" : "disabled") + ".");
        logger.LogInfo("Paired broadcaster ID present: " + (!string.IsNullOrWhiteSpace(settings.BroadcasterUserId.Value)) + ".");
        logger.LogInfo("Saved host credential present: " + (!string.IsNullOrWhiteSpace(settings.HostToken.Value)) + ".");
        logger.LogInfo("Verbose diagnostics: " + (settings.VerboseLogging.Value ? "enabled" : "disabled") + ".");
        logger.LogInfo("PEAK lobby/player adapter is not registered yet; no gameplay state is read or changed.");
    }
}
