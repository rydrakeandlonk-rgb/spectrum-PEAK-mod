using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Configuration;

namespace Spectrum.PEAK.Connection;

/// <summary>
/// Handles the Spectrum device-code pairing and authenticated host connection.
/// It deliberately does not inspect or modify PEAK gameplay state.
/// </summary>
internal sealed class PeakConnectionClient : IDisposable
{
    private const string ServiceBaseUrl = "https://spectrum-game-mods.rydrakeandlonk.workers.dev";
    private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

    private readonly ConfigEntry<bool> _enabled;
    private readonly ConfigEntry<string> _hostToken;
    private readonly ConfigEntry<string> _broadcasterId;
    private readonly CancellationTokenSource _shutdown = new CancellationTokenSource();
    private Task? _runner;

    public PeakConnectionClient(
        ConfigEntry<bool> enabled,
        ConfigEntry<string> hostToken,
        ConfigEntry<string> broadcasterId)
    {
        _enabled = enabled;
        _hostToken = hostToken;
        _broadcasterId = broadcasterId;
    }

    public void Start()
    {
        if (!_enabled.Value)
        {
            Plugin.Log.LogInfo("Spectrum connection is disabled in the plugin configuration.");
            return;
        }

        if (_runner != null) return;
        _runner = Task.Run(() => RunAsync(_shutdown.Token));
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_hostToken.Value) ||
                    string.IsNullOrWhiteSpace(_broadcasterId.Value))
                {
                    await PairDeviceAsync(cancellationToken);
                }

                await ConnectAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                Plugin.Log.LogWarning("Spectrum connection interrupted: " + exception.Message);
            }

            if (!cancellationToken.IsCancellationRequested)
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }
    }

    private async Task PairDeviceAsync(CancellationToken cancellationToken)
    {
        using var response = await Http.PostAsync(
            ServiceBaseUrl + "/connect/peak/pairing/start",
            new StringContent("{}", Encoding.UTF8, "application/json"),
            cancellationToken);
        response.EnsureSuccessStatusCode();

        using var startJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = startJson.RootElement;
        var userCode = root.GetProperty("user_code").GetString() ?? throw new InvalidOperationException("Pairing code was missing.");
        var deviceCode = root.GetProperty("device_code").GetString() ?? throw new InvalidOperationException("Device code was missing.");
        var verificationUri = root.GetProperty("verification_uri").GetString() ?? ServiceBaseUrl;

        Plugin.Log.LogInfo("Spectrum pairing required. Open " + verificationUri + " and enter code " + userCode + ".");
        var pollUri = ServiceBaseUrl + "/connect/peak/pairing/poll";

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            var body = JsonSerializer.Serialize(new { user_code = userCode, device_code = deviceCode });
            using var pollResponse = await Http.PostAsync(
                pollUri,
                new StringContent(body, Encoding.UTF8, "application/json"),
                cancellationToken);
            pollResponse.EnsureSuccessStatusCode();

            using var pollJson = JsonDocument.Parse(await pollResponse.Content.ReadAsStringAsync());
            var poll = pollJson.RootElement;
            var status = poll.TryGetProperty("status", out var statusElement)
                ? statusElement.GetString()
                : "unknown";

            if (status == "pending") continue;
            if (status == "expired") throw new InvalidOperationException("Spectrum pairing code expired. Restart the plugin to request a new code.");
            if (status != "authorized") throw new InvalidOperationException("Unexpected Spectrum pairing status: " + status);

            _hostToken.Value = poll.GetProperty("host_token").GetString() ?? "";
            _broadcasterId.Value = poll.GetProperty("broadcaster_user_id").GetString() ?? "";
            Plugin.Settings.Save();
            Plugin.Log.LogInfo("Spectrum pairing approved for Twitch account " +
                (poll.TryGetProperty("broadcaster_login", out var login) ? login.GetString() : "connected account") + ".");
            return;
        }
    }

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        var broadcasterId = _broadcasterId.Value.Trim();
        if (string.IsNullOrWhiteSpace(broadcasterId) || string.IsNullOrWhiteSpace(_hostToken.Value))
            throw new InvalidOperationException("No saved PEAK pairing credentials are available.");

        var sessionId = Guid.NewGuid().ToString("N");
        var builder = new UriBuilder(ServiceBaseUrl)
        {
            Scheme = "wss",
            Path = "/connect/peak/" + Uri.EscapeDataString(broadcasterId),
            Query = "session_id=" + sessionId
        };

        using var socket = new ClientWebSocket();
        socket.Options.SetRequestHeader("Authorization", "Bearer " + _hostToken.Value);
        await socket.ConnectAsync(builder.Uri, cancellationToken);
        Plugin.Log.LogInfo("Connected to Spectrum Game Mods. Waiting for PEAK session reporting.");

        var readyBuffer = new byte[2048];
        using (var readyTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            readyTimeout.CancelAfter(TimeSpan.FromSeconds(10));
            var ready = await socket.ReceiveAsync(new ArraySegment<byte>(readyBuffer), readyTimeout.Token);
            var readyText = Encoding.UTF8.GetString(readyBuffer, 0, ready.Count);
            if (!readyText.Contains("session.ready"))
                throw new InvalidOperationException("Spectrum did not confirm the PEAK session handshake.");
        }

        while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var ping = Encoding.UTF8.GetBytes("{\"type\":\"session.ping\"}");
            await socket.SendAsync(new ArraySegment<byte>(ping), WebSocketMessageType.Text, true, cancellationToken);

            using var receiveTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            receiveTimeout.CancelAfter(TimeSpan.FromSeconds(25));
            try
            {
                var buffer = new byte[1024];
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), receiveTimeout.Token);
                if (result.MessageType == WebSocketMessageType.Close) break;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("Spectrum session heartbeat timed out.");
            }

            await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
        }
    }

    public void Dispose()
    {
        _shutdown.Cancel();
        try { _runner?.Wait(TimeSpan.FromSeconds(2)); } catch { }
        _shutdown.Dispose();
    }
}
