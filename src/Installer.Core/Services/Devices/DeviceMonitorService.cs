using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Core.Services.Devices;

public sealed class DeviceMonitorService : IDeviceMonitorService
{
    public const int FailuresBeforeEmpty = 3;

    private readonly IDeviceService _deviceService;
    private readonly IAppLogger _logger;
    private readonly TimeSpan _interval;
    private CancellationTokenSource? _cts;
    private Task? _loop;

    public DeviceMonitorService(IDeviceService deviceService, IAppLogger logger, TimeSpan? interval = null)
    {
        _deviceService = deviceService;
        _logger = logger;
        _interval = interval ?? TimeSpan.FromSeconds(2);
        CurrentDevices = [];
    }

    public event EventHandler<IReadOnlyList<DeviceInfo>>? DevicesChanged;

    public IReadOnlyList<DeviceInfo> CurrentDevices { get; private set; }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        Stop();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loop = RunAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public void Stop()
    {
        try
        {
            _cts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // ignored
        }

        _cts?.Dispose();
        _cts = null;
        _loop = null;
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        var failures = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var devices = await _deviceService.TryDetectAsync(cancellationToken);
                if (devices is null)
                {
                    // One failed adb call is not an unplug; keep the last list until failures persist.
                    failures++;
                    devices = failures >= FailuresBeforeEmpty ? [] : null;
                }
                else
                {
                    failures = 0;
                }

                if (devices is not null && !cancellationToken.IsCancellationRequested)
                {
                    CurrentDevices = devices;
                    DevicesChanged?.Invoke(this, devices);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Error("Device poll failed.", ex);
            }

            try
            {
                await Task.Delay(_interval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
