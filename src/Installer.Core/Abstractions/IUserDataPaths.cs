namespace Installer.Core.Abstractions;

public interface IUserDataPaths
{
    string DiagnosticsDirectory { get; }
    string WirelessEndpointPath { get; }
    string RecentsPath { get; }
    string ReportRecipientPath { get; }
}
