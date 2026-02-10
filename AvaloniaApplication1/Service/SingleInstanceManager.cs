// Author: wangchaozhi
// Date: 2026/02/10

using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaApplication1.Service;

public static class SingleInstanceManager
{
    private static Mutex? _mutex;
    private static CancellationTokenSource? _listenCts;

    public static string? InstanceKey { get; private set; }

    public static bool InitializeAsFirstInstance(string instanceKey)
    {
        if (string.IsNullOrWhiteSpace(instanceKey))
            throw new ArgumentException("Instance key cannot be null or whitespace.", nameof(instanceKey));

        InstanceKey = instanceKey;

        // One instance per user session.
        // Note: Do not use Global\ prefix to avoid privilege issues.
        _mutex = new Mutex(initiallyOwned: true, name: instanceKey, createdNew: out var createdNew);

        if (!createdNew)
        {
            _mutex.Dispose();
            _mutex = null;
            return false;
        }

        return true;
    }

    public static async Task SignalFirstInstanceAsync(string instanceKey, int totalTimeoutMs = 2000)
    {
        if (string.IsNullOrWhiteSpace(instanceKey))
            return;

        // The first instance might still be starting up and hasn't created the pipe server yet.
        // Retry a few times within the timeout window.
        var start = Environment.TickCount64;
        while (Environment.TickCount64 - start < totalTimeoutMs)
        {
            try
            {
                await using var client = new NamedPipeClientStream(
                    serverName: ".",
                    pipeName: instanceKey,
                    direction: PipeDirection.Out,
                    options: PipeOptions.Asynchronous);

                using var connectCts = new CancellationTokenSource(millisecondsDelay: 250);
                await client.ConnectAsync(connectCts.Token);

                await using var writer = new StreamWriter(client) { AutoFlush = true };
                await writer.WriteLineAsync("ACTIVATE");
                return;
            }
            catch
            {
                await Task.Delay(100);
            }
        }
    }

    public static void StartListening(string instanceKey, Action onActivate)
    {
        if (string.IsNullOrWhiteSpace(instanceKey))
            throw new ArgumentException("Instance key cannot be null or whitespace.", nameof(instanceKey));

        _listenCts ??= new CancellationTokenSource();
        var token = _listenCts.Token;

        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await using var server = new NamedPipeServerStream(
                        pipeName: instanceKey,
                        direction: PipeDirection.In,
                        maxNumberOfServerInstances: 1,
                        transmissionMode: PipeTransmissionMode.Byte,
                        options: PipeOptions.Asynchronous);

                    await server.WaitForConnectionAsync(token);

                    using var reader = new StreamReader(server);
                    var line = await reader.ReadLineAsync();
                    if (string.Equals(line, "ACTIVATE", StringComparison.OrdinalIgnoreCase))
                    {
                        onActivate();
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch
                {
                    // Ignore transient pipe errors and keep listening.
                    try
                    {
                        await Task.Delay(100, token);
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }, token);
    }

    public static void Shutdown()
    {
        try
        {
            _listenCts?.Cancel();
        }
        catch
        {
            // ignored
        }

        _listenCts?.Dispose();
        _listenCts = null;

        try
        {
            _mutex?.ReleaseMutex();
        }
        catch
        {
            // ignored
        }

        _mutex?.Dispose();
        _mutex = null;
        InstanceKey = null;
    }
}
