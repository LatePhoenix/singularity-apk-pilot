using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IUsbEvidenceProbe
{
    UsbEvidence Collect();
}
