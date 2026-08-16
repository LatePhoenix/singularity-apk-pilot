using System.Windows;
using Installer.App.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace Installer.App;

public partial class App : Application
{
    private IServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _services = ServiceRegistration.Create();
        var window = _services.GetRequiredService<Views.ShellWindow>();
        window.Show();
    }
}
