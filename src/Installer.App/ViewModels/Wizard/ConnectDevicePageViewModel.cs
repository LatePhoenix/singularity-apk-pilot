using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public sealed record WirelessFormRequest(WirelessEndpoint Connect, WirelessEndpoint? Pairing, string? PairingCode);

public sealed partial class ConnectDevicePageViewModel : WizardPageViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConnectAdvanced))]
    private string address = "";

    [ObservableProperty]
    private string pairingPort = "";

    [ObservableProperty]
    private string pairingCode = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConnectRemembered))]
    private bool hasRememberedEndpoint;

    [ObservableProperty]
    private string rememberedLabel = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConnectRemembered))]
    [NotifyPropertyChangedFor(nameof(CanConnectAdvanced))]
    private bool isWifiBusy;

    [ObservableProperty]
    private string wifiStatus = "";

    public bool CanConnectRemembered => HasRememberedEndpoint && !IsWifiBusy;

    public bool CanConnectAdvanced => !IsWifiBusy && !string.IsNullOrWhiteSpace(Address);

    public event Action? ConnectRememberedRequested;

    public event Action<WirelessFormRequest>? ConnectAdvancedRequested;

    public void BindEndpoint(WirelessEndpoint? endpoint)
    {
        HasRememberedEndpoint = endpoint is not null;
        RememberedLabel = endpoint is null ? "" : $"Last used: {endpoint.Address}";
        if (endpoint is not null && string.IsNullOrWhiteSpace(Address))
        {
            Address = endpoint.Address;
        }
    }

    [RelayCommand]
    private void ConnectRemembered()
    {
        if (CanConnectRemembered)
        {
            ConnectRememberedRequested?.Invoke();
        }
    }

    [RelayCommand]
    private void ConnectAdvanced()
    {
        if (!WirelessEndpoint.TryParse(Address, out var connect))
        {
            WifiStatus = "Enter a Wi-Fi address like 192.168.1.42:5555.";
            return;
        }

        WirelessEndpoint? pairing = null;
        var code = string.IsNullOrWhiteSpace(PairingCode) ? null : PairingCode.Trim();
        if (!string.IsNullOrWhiteSpace(code))
        {
            if (!int.TryParse(PairingPort.Trim(), out var port) || port is <= 0 or > 65535)
            {
                WifiStatus = "Pairing needs a pairing port from the device, plus the six-digit code.";
                return;
            }

            pairing = new WirelessEndpoint(connect.Host, port);
        }

        WifiStatus = "";
        ConnectAdvancedRequested?.Invoke(new WirelessFormRequest(connect, pairing, code));
    }
}
