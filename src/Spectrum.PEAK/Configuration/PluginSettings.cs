using BepInEx.Configuration;

namespace Spectrum.PEAK.Configuration;

/// <summary>
/// Local diagnostic and feature-gate settings.
/// Network integration remains disabled until the shared Spectrum protocol
/// and PEAK session APIs have been verified.
/// </summary>
internal sealed class PluginSettings
{
    public ConfigEntry<bool> EnableIntegration { get; }
    public ConfigEntry<bool> VerboseLogging { get; }

    public PluginSettings(ConfigFile config)
    {
        EnableIntegration = config.Bind(
            "General",
            "EnableIntegration",
            false,
            "Reserved feature gate. Keep disabled until the Spectrum session protocol is implemented and tested.");

        VerboseLogging = config.Bind(
            "Diagnostics",
            "VerboseLogging",
            false,
            "Enable additional diagnostic logging. Do not log credentials or private payloads.");
    }
}
