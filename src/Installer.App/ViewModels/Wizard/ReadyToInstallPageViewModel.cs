using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Installer.Core.Abstractions;
using Installer.Core.Models;
using Microsoft.Win32;

namespace Installer.App.ViewModels.Wizard;

public sealed record ApkFileItem(string Path, string Name, string Summary, string Warning);

public sealed partial class ReadyToInstallPageViewModel : WizardPageViewModel
{
    private readonly IApkInspector _inspector;
    private readonly IRecentsStore _recents;
    private readonly IInstallSetFactory _sets;

    public ReadyToInstallPageViewModel(IApkInspector inspector, IRecentsStore recents, IInstallSetFactory sets)
    {
        _inspector = inspector;
        _recents = recents;
        _sets = sets;
        ApkFiles.CollectionChanged += (_, _) =>
        {
            HasFiles = ApkFiles.Count > 0;
            RefreshWarnings();
            FilesChanged?.Invoke();
        };
        var loaded = _recents.Load();
        LastFolder = loaded.LastFolder;
        RecentFiles = loaded.LastFiles;
        HasRecents = RecentFiles.Count > 0;
    }

    public ObservableCollection<ApkFileItem> ApkFiles { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptyHint))]
    [NotifyPropertyChangedFor(nameof(ShowUseLastFiles))]
    private bool hasFiles;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanUseWifi))]
    private bool showUseWifi;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanUseWifi))]
    private bool isWifiBusy;

    [ObservableProperty]
    private bool showWifiConnected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowUseLastFiles))]
    private bool hasRecents;

    [ObservableProperty]
    private string splitWarning = "";

    public string EmptyHint => HasFiles ? "" : "No APK files selected yet.";

    public bool CanUseWifi => ShowUseWifi && !IsWifiBusy;

    public bool ShowUseLastFiles => HasRecents && !HasFiles;

    public IReadOnlyList<string> RecentFiles { get; private set; } = [];

    public string? LastFolder { get; private set; }

    public event Action? FilesChanged;

    public event Action? UseWifiRequested;

    public IReadOnlyList<string> SelectedPaths => ApkFiles.Select(file => file.Path).ToList();

    public void ClearFiles() => ApkFiles.Clear();

    public void AddPaths(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            AddPath(path);
        }
    }

    protected override void OnApplied(WizardState state)
    {
        var ready = state.Device is { State: DeviceConnectionState.ConnectedReady };
        ShowUseWifi = ready && state.Device is { IsWireless: false };
        ShowWifiConnected = ready && state.Device is { IsWireless: true };
    }

    [RelayCommand]
    private void UseWifi()
    {
        if (CanUseWifi)
        {
            UseWifiRequested?.Invoke();
        }
    }

    [RelayCommand]
    private void UseLastFiles()
    {
        AddPaths(RecentFiles);
    }

    [RelayCommand]
    private void AddApks()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose app files",
            Filter = "Android packages (*.apk;*.apks;*.xapk)|*.apk;*.apks;*.xapk|All files (*.*)|*.*",
            Multiselect = true,
            CheckFileExists = true,
            InitialDirectory = LastFolder ?? ""
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        AddPaths(dialog.FileNames);
    }

    [RelayCommand]
    private void RemoveApk(ApkFileItem? item)
    {
        if (item is not null)
        {
            ApkFiles.Remove(item);
        }
    }

    private void AddPath(string path)
    {
        var ext = Path.GetExtension(path);
        if (string.IsNullOrWhiteSpace(path)
            || !File.Exists(path)
            || !IsPackageExtension(ext)
            || ApkFiles.Any(existing => string.Equals(existing.Path, path, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var identity = _inspector.Inspect(path);
        ApkFiles.Add(new ApkFileItem(
            path,
            Path.GetFileName(path),
            identity?.Summary ?? Path.GetFileName(path),
            identity is { IsSplit: true } ? "This looks like only part of an app." : ""));
        LastFolder = Path.GetDirectoryName(path);
        Persist();
    }

    private void RefreshWarnings()
    {
        SplitWarning = !HasFiles
            ? ""
            : _sets.Group(SelectedPaths).Any(set => set.LooksLikeMissingSplits)
                ? "This looks like only part of an app. Add the other files or an .apks package."
                : "";
    }

    private void Persist()
    {
        var files = SelectedPaths;
        RecentFiles = files.Count > 0 ? files : RecentFiles;
        HasRecents = RecentFiles.Count > 0;
        if (files.Count > 0)
        {
            _recents.Save(new RecentsState(LastFolder, files));
        }
    }

    private static bool IsPackageExtension(string ext) =>
        ext.Equals(".apk", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".apks", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".xapk", StringComparison.OrdinalIgnoreCase);
}
