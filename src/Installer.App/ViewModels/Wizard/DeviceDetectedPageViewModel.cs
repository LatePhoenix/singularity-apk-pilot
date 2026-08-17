using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public sealed partial class DeviceDetectedPageViewModel : WizardPageViewModel
{
    public ObservableCollection<DeviceInfo> Candidates { get; } = [];

    [ObservableProperty]
    private string deviceSummary = "";

    [ObservableProperty]
    private bool showPicker;

    [ObservableProperty]
    private DeviceInfo? selectedDevice;

    public event Action<DeviceInfo>? DeviceChosen;

    protected override void OnApplied(WizardState state)
    {
        Candidates.Clear();
        var ready = state.Ready.Where(d => d.State == DeviceConnectionState.ConnectedReady).ToList();
        ShowPicker = ready.Count >= 2;
        foreach (var device in ready)
        {
            Candidates.Add(device);
        }

        SelectedDevice = state.Device is not null && ready.Any(d => d.Serial == state.Device.Serial)
            ? ready.First(d => d.Serial == state.Device.Serial)
            : ready.FirstOrDefault() ?? state.Device;
        DeviceSummary = SelectedDevice is null
            ? ""
            : $"{SelectedDevice.DisplayName} · {SelectedDevice.Kind}" +
              (SelectedDevice.IsWireless ? " · Wi-Fi" : "");
    }

    [RelayCommand]
    private void Choose(DeviceInfo? device)
    {
        if (device is null)
        {
            return;
        }

        SelectedDevice = device;
        DeviceSummary = $"{device.DisplayName} · {device.Kind}" + (device.IsWireless ? " · Wi-Fi" : "");
        DeviceChosen?.Invoke(device);
    }
}
