using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public sealed partial class InstalledAppRow : ObservableObject
{
    public InstalledAppRow(InstalledApp app)
    {
        App = app;
    }

    public InstalledApp App { get; private set; }

    public string PackageId => App.PackageId;

    public string DisplayName => App.DisplayName;

    public string Summary => App.Summary;

    public bool IsRecent => App.IsRecent;

    [ObservableProperty]
    private bool isRemoving;

    public void Update(InstalledApp app)
    {
        App = app;
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(Summary));
        OnPropertyChanged(nameof(IsRecent));
    }
}

public sealed partial class InstalledAppsPageViewModel : WizardPageViewModel
{
    private IReadOnlyList<InstalledAppRow> _all = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptyHint))]
    [NotifyPropertyChangedFor(nameof(ShowList))]
    private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRemove))]
    [NotifyPropertyChangedFor(nameof(CanCancelUninstall))]
    private bool isBusy;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private string statusMessage = "";

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowConfirm))]
    [NotifyPropertyChangedFor(nameof(CanRemove))]
    private InstalledAppRow? pendingRemove;

    public ObservableCollection<InstalledAppRow> VisibleApps { get; } = [];

    public bool ShowConfirm => PendingRemove is not null;

    public bool ShowList => !IsLoading;

    public bool CanRemove => ShowConfirm && !IsBusy;

    public bool CanCancelUninstall => IsBusy;

    public string EmptyHint =>
        IsLoading ? "Reading apps on the device…"
        : VisibleApps.Count > 0 ? ""
        : string.IsNullOrWhiteSpace(SearchText) ? "No third-party apps were found."
        : "No apps match that search.";

    public string ConfirmTitle => PendingRemove is null ? "" : $"Remove {PendingRemove.DisplayName}?";

    public string ConfirmBody =>
        PendingRemove is null
            ? ""
            : $"{PendingRemove.PackageId} will be removed from the device, including its data. Store apps can be reinstalled from the store. Sideloaded apps need the APK again.";

    public event Action? RefreshRequested;

    public event Action<string>? UninstallRequested;

    public event Action? CancelUninstallRequested;

    public event Action? EnrichVisibleRequested;

    public IReadOnlyList<InstalledApp> VisibleModels => VisibleApps.Select(row => row.App).ToList();

    public void Bind(IReadOnlyList<InstalledApp> apps)
    {
        _all = apps.Select(app => new InstalledAppRow(app)).ToList();
        IsLoading = false;
        ApplyFilter();
    }

    public void MergeEnrichment(IReadOnlyList<InstalledApp> enriched)
    {
        var byId = enriched.ToDictionary(app => app.PackageId, StringComparer.OrdinalIgnoreCase);
        foreach (var row in _all)
        {
            if (byId.TryGetValue(row.PackageId, out var app))
            {
                row.Update(app);
            }
        }

        OnPropertyChanged(nameof(ConfirmTitle));
        OnPropertyChanged(nameof(ConfirmBody));
    }

    public void BeginUninstall(string packageId)
    {
        IsBusy = true;
        ErrorMessage = "";
        StatusMessage = "Removing…";
        foreach (var row in VisibleApps)
        {
            row.IsRemoving = row.PackageId.Equals(packageId, StringComparison.OrdinalIgnoreCase);
        }
    }

    public void EndUninstall(UninstallResult result)
    {
        IsBusy = false;
        foreach (var row in _all)
        {
            row.IsRemoving = false;
        }

        if (result.Success)
        {
            _all = _all.Where(row => !row.PackageId.Equals(result.PackageId, StringComparison.OrdinalIgnoreCase)).ToList();
            PendingRemove = null;
            StatusMessage = "Removed.";
            ErrorMessage = "";
            ApplyFilter();
            return;
        }

        StatusMessage = "";
        ErrorMessage = result.Message;
    }

    public void CancelBusy()
    {
        IsBusy = false;
        StatusMessage = "";
        foreach (var row in _all)
        {
            row.IsRemoving = false;
        }
    }

    protected override void OnApplied(WizardState state)
    {
        if (state.CurrentStep != WizardStep.InstalledApps)
        {
            return;
        }

        OnPropertyChanged(nameof(ConfirmTitle));
        OnPropertyChanged(nameof(ConfirmBody));
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    [RelayCommand]
    private void Refresh()
    {
        if (IsBusy)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = "";
        StatusMessage = "";
        RefreshRequested?.Invoke();
    }

    [RelayCommand]
    private void AskRemove(InstalledAppRow? row)
    {
        if (row is null || IsBusy)
        {
            return;
        }

        PendingRemove = row;
        ErrorMessage = "";
        OnPropertyChanged(nameof(ConfirmTitle));
        OnPropertyChanged(nameof(ConfirmBody));
    }

    [RelayCommand]
    private void CancelConfirm()
    {
        if (!IsBusy)
        {
            PendingRemove = null;
        }
    }

    [RelayCommand]
    private void ConfirmRemove()
    {
        if (PendingRemove is null || IsBusy)
        {
            return;
        }

        UninstallRequested?.Invoke(PendingRemove.PackageId);
    }

    [RelayCommand]
    private void CancelUninstall()
    {
        if (IsBusy)
        {
            CancelUninstallRequested?.Invoke();
        }
    }

    private void ApplyFilter()
    {
        var query = SearchText.Trim();
        VisibleApps.Clear();
        foreach (var row in _all)
        {
            if (query.Length == 0
                || row.PackageId.Contains(query, StringComparison.OrdinalIgnoreCase)
                || row.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                VisibleApps.Add(row);
            }
        }

        OnPropertyChanged(nameof(EmptyHint));
        OnPropertyChanged(nameof(ShowList));
        EnrichVisibleRequested?.Invoke();
    }
}
