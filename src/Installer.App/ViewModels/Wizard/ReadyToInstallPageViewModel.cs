using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace Installer.App.ViewModels.Wizard;

public sealed record ApkFileItem(string Path, string Name);

public sealed partial class ReadyToInstallPageViewModel : WizardPageViewModel
{
    public ObservableCollection<ApkFileItem> ApkFiles { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptyHint))]
    private bool hasFiles;

    public string EmptyHint => HasFiles ? "" : "No APK files selected yet.";

    public event Action? FilesChanged;

    public ReadyToInstallPageViewModel()
    {
        ApkFiles.CollectionChanged += (_, _) =>
        {
            HasFiles = ApkFiles.Count > 0;
            FilesChanged?.Invoke();
        };
    }

    public IReadOnlyList<string> SelectedPaths => ApkFiles.Select(file => file.Path).ToList();

    public void ClearFiles() => ApkFiles.Clear();

    [RelayCommand]
    private void AddApks()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose APK files",
            Filter = "Android packages (*.apk)|*.apk|All files (*.*)|*.*",
            Multiselect = true,
            CheckFileExists = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        foreach (var path in dialog.FileNames)
        {
            if (ApkFiles.Any(existing => string.Equals(existing.Path, path, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            ApkFiles.Add(new ApkFileItem(path, System.IO.Path.GetFileName(path)));
        }
    }

    [RelayCommand]
    private void RemoveApk(ApkFileItem? item)
    {
        if (item is not null)
        {
            ApkFiles.Remove(item);
        }
    }
}
