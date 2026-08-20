using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public sealed record WirelessFormRequest(WirelessEndpoint Connect, WirelessEndpoint? Pairing, string? PairingCode);

public sealed record GuideStep(string Number, string Text);

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
    [NotifyPropertyChangedFor(nameof(ConnectingLabel))]
    private bool isWifiBusy;

    [ObservableProperty]
    private string wifiStatus = "";

    [ObservableProperty]
    private string healthHint = "";

    public bool CanConnectRemembered => HasRememberedEndpoint && !IsWifiBusy;

    public bool CanConnectAdvanced => !IsWifiBusy && !string.IsNullOrWhiteSpace(Address);

    public string ConnectingLabel => IsWifiBusy ? "Connecting over Wi-Fi…" : "";

    public IReadOnlyList<GuideStep> QuestWifiSteps { get; } =
    [
        new("1", "On your phone, open the Meta Horizon app. Tap the headset icon, then your Quest 2, Quest 3, Quest 3S, or Quest Pro, then Headset Settings, then Developer Mode, and turn it on."),
        new("2", "Plug a USB-C data cable into the headset and this computer. The cable in the Quest box is often charge-only — use one that can transfer files."),
        new("3", "Put the headset on. Open Quick Control, then Settings (gear), then Developer, and turn on MTP Notification."),
        new("4", "When asked, choose Always allow from this computer, then Allow. Wait until this installer shows the headset is ready."),
        new("5", "On Choose apps, tap Switch to Wi-Fi, then unplug. Next time, tap Connect over Wi-Fi on this screen. After a headset reboot, plug in once more.")
    ];

    public IReadOnlyList<GuideStep> PairingSteps { get; } =
    [
        new("1", "Put the headset and this computer on the same Wi-Fi. Guest networks will not work."),
        new("2", "Put the headset on. Open Settings, then System, then Developer. Turn on wireless debugging if you see it."),
        new("3", "Note the IP address and port shown for connecting. That is the install address — not the pairing port."),
        new("4", "If this computer has never connected over Wi-Fi, tap Pair device with pairing code. Enter that pairing port and the six-digit code below. Those numbers expire quickly."),
        new("5", "Enter the install address below. Add pairing details only if you just paired, then tap Connect over Wi-Fi.")
    ];

    public event Action? ConnectRememberedRequested;

    public event Action<WirelessFormRequest>? ConnectAdvancedRequested;

    public void BindEndpoint(WirelessEndpoint? endpoint)
    {
        HasRememberedEndpoint = endpoint is not null;
        RememberedLabel = endpoint is null ? "" : endpoint.Address;
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
            if (code.Length != 6 || !code.All(char.IsDigit))
            {
                WifiStatus = "Pairing needs the six-digit code from the device.";
                return;
            }

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

    protected override void OnApplied(WizardState state)
    {
        HealthHint = state.Health?.Hint ?? "";
    }
}
