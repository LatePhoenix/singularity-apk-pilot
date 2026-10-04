using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Devices;

namespace Installer.Core.Tests.Devices;

public sealed class DeviceMonitorServiceTests
{
    [Fact]
    public async Task Transient_adb_failure_keeps_the_last_device_list()
    {
        var quest = Device();
        var service = new ScriptedDevices([[quest], null, null, [quest]]);
        var monitor = new DeviceMonitorService(service, new NoopLog(), TimeSpan.FromMilliseconds(5));
        var published = new List<IReadOnlyList<DeviceInfo>>();
        monitor.DevicesChanged += (_, devices) => published.Add(devices);

        await monitor.StartAsync();
        await service.Drained.Task.WaitAsync(TimeSpan.FromSeconds(5));
        monitor.Stop();

        Assert.Equal(2, published.Count);
        Assert.All(published, devices => Assert.Single(devices));
    }

    [Fact]
    public async Task Persistent_adb_failure_eventually_reports_no_devices()
    {
        var quest = Device();
        var results = new List<IReadOnlyList<DeviceInfo>?> { new[] { quest } };
        results.AddRange(Enumerable.Repeat<IReadOnlyList<DeviceInfo>?>(null, DeviceMonitorService.FailuresBeforeEmpty));
        var service = new ScriptedDevices(results);
        var monitor = new DeviceMonitorService(service, new NoopLog(), TimeSpan.FromMilliseconds(5));
        var published = new List<IReadOnlyList<DeviceInfo>>();
        monitor.DevicesChanged += (_, devices) => published.Add(devices);

        await monitor.StartAsync();
        await service.Drained.Task.WaitAsync(TimeSpan.FromSeconds(5));
        monitor.Stop();

        Assert.Equal(2, published.Count);
        Assert.Empty(published[1]);
        Assert.Empty(monitor.CurrentDevices);
    }

    [Fact]
    public async Task Stop_during_a_poll_does_not_publish()
    {
        var service = new BlockingDevices();
        var monitor = new DeviceMonitorService(service, new NoopLog(), TimeSpan.FromMilliseconds(5));
        var published = 0;
        monitor.DevicesChanged += (_, _) => published++;

        await monitor.StartAsync();
        await service.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        monitor.Stop();
        await Task.Delay(50);

        Assert.Equal(0, published);
    }

    private static DeviceInfo Device() =>
        new("1WMHH000000001", "Oculus", "Quest 3", "14", DeviceKind.MetaQuest,
            DeviceConnectionState.ConnectedReady, true, true, new Dictionary<string, string>());

    private sealed class ScriptedDevices : IDeviceService
    {
        private readonly Queue<IReadOnlyList<DeviceInfo>?> _results;

        public ScriptedDevices(IEnumerable<IReadOnlyList<DeviceInfo>?> results)
        {
            _results = new Queue<IReadOnlyList<DeviceInfo>?>(results);
        }

        public TaskCompletionSource Drained { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<IReadOnlyList<DeviceInfo>> DetectAsync(CancellationToken cancellationToken = default) =>
            await TryDetectAsync(cancellationToken) ?? [];

        public async Task<IReadOnlyList<DeviceInfo>?> TryDetectAsync(CancellationToken cancellationToken = default)
        {
            if (_results.Count == 0)
            {
                Drained.TrySetResult();
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }

            var next = _results.Dequeue();
            if (_results.Count == 0)
            {
                // Let the monitor publish this last result before the test stops it.
                _ = Task.Delay(50).ContinueWith(_ => Drained.TrySetResult(), TaskScheduler.Default);
            }

            return next;
        }

        public DeviceInfo? SelectPrimary(IReadOnlyList<DeviceInfo> devices) => devices.FirstOrDefault();
    }

    private sealed class BlockingDevices : IDeviceService
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<IReadOnlyList<DeviceInfo>> DetectAsync(CancellationToken cancellationToken = default) =>
            await TryDetectAsync(cancellationToken) ?? [];

        public async Task<IReadOnlyList<DeviceInfo>?> TryDetectAsync(CancellationToken cancellationToken = default)
        {
            Entered.TrySetResult();
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Old behavior: detection swallowed cancellation and returned an empty list.
            }

            return [];
        }

        public DeviceInfo? SelectPrimary(IReadOnlyList<DeviceInfo> devices) => devices.FirstOrDefault();
    }

    private sealed class NoopLog : IAppLogger
    {
        public void Info(string message) { }

        public void Warn(string message) { }

        public void Error(string message, Exception? exception = null) { }
    }
}
