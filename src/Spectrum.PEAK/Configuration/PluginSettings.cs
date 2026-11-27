using BepInEx.Configuration;

namespace Spectrum.PEAK.Configuration;

/// <summary>Local connection and diagnostic settings.</summary>
internal sealed class PluginSettings
{
    private readonly ConfigFile _config;

    public ConfigEntry<bool> EnableIntegration { get; }
    public ConfigEntry<bool> VerboseLogging { get; }
    public ConfigEntry<string> HostToken { get; }
    public ConfigEntry<string> BroadcasterUserId { get; }

    public PluginSettings(ConfigFile config)
    {
        _config = config;
        EnableIntegration = config.Bind(
            "Connection",
            "EnableIntegration",
            false,
            "Enable the Spectrum connection and device-code pairing flow.");

        HostToken = config.Bind(
            "Connection",
            "HostToken",
            "",
            "Private credential issued by Spectrum during pairing. Do not share this value.");

        BroadcasterUserId = config.Bind(
            "Connection",
            "BroadcasterUserId",
            "",
            "Twitch account ID associated with this paired PEAK installation.");

        VerboseLogging = config.Bind(
            "Diagnostics",
            "VerboseLogging",
            false,
            "Enable additional diagnostic logging. Credentials are never logged.");
    }

    public void Save() => _config.Save();
}
