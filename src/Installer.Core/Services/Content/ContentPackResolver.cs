using Installer.Core.Abstractions;

namespace Installer.Core.Services.Content;

public sealed class ContentPackResolver
{
    private readonly IPayloadLocator _payloads;

    public ContentPackResolver(IPayloadLocator payloads)
    {
        _payloads = payloads;
    }

    public string CurrentRoot => _payloads.PayloadRoot;
}
