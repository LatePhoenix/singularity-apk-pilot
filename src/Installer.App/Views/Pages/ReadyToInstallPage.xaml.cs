using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Installer.App.ViewModels.Wizard;

namespace Installer.App.Views.Pages;

public partial class ReadyToInstallPage : UserControl
{
    public ReadyToInstallPage()
    {
        InitializeComponent();
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
        SetDropHighlight(e.Effects == DragDropEffects.Copy);
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        SetDropHighlight(false);
        e.Handled = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        SetDropHighlight(false);
        if (DataContext is not ReadyToInstallPageViewModel page
            || e.Data.GetData(DataFormats.FileDrop) is not string[] files)
        {
            return;
        }

        page.AddPaths(files.Where(path =>
        {
            var ext = Path.GetExtension(path);
            return ext.Equals(".apk", StringComparison.OrdinalIgnoreCase)
                   || ext.Equals(".apks", StringComparison.OrdinalIgnoreCase)
                   || ext.Equals(".xapk", StringComparison.OrdinalIgnoreCase);
        }));
        e.Handled = true;
    }

    private void SetDropHighlight(bool active)
    {
        if (DropZoneBorder is null)
        {
            return;
        }

        if (active)
        {
            DropZoneBorder.SetResourceReference(Border.BorderBrushProperty, "BrandCyanBrush");
            DropZoneBorder.BorderThickness = new Thickness(2);
        }
        else
        {
            DropZoneBorder.SetResourceReference(Border.BorderBrushProperty, "BorderBrushStrong");
            DropZoneBorder.BorderThickness = new Thickness(1);
        }
    }
}
