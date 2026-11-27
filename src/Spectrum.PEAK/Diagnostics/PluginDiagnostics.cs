using BepInEx.Logging;
using Spectrum.PEAK.Configuration;

namespace Spectrum.PEAK.Diagnostics;

/// <summary>Startup diagnostics that do not inspect or mutate PEAK gameplay state.</summary>
internal static class PluginDiagnostics
{
    public static void LogStartup(ManualLogSource logger, PluginSettings settings)
    {
        logger.LogInfo("Runtime: BepInEx plugin bootstrap is active.");
        logger.LogInfo($"Integration feature gate: {(settings.EnableIntegration.Value ? "enabled" : "disabled")}.");
        logger.LogInfo($"Verbose diagnostics: {(settings.VerboseLogging.Value ? "enabled" : "disabled")}.");
        logger.LogInfo("PEAK game adapters are not registered yet; game APIs are pending assembly inspection.");
        logger.LogInfo("No Twitch credentials, donation data, or network endpoints are stored by this plugin bootstrap.");
    }
}
