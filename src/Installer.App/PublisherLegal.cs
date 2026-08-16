namespace Installer.App;

public static class PublisherLegal
{
    public const string HomeUrl = "https://singularity.mhbross725.workers.dev/";
    public const string PrivacyUrl = "https://singularity.mhbross725.workers.dev/privacy";
    public const string TermsUrl = "https://singularity.mhbross725.workers.dev/terms";
    public const string PublisherEmail = "matt.brossard323@gmail.com";

    public static Uri HomeUri { get; } = new(HomeUrl);
    public static Uri PrivacyUri { get; } = new(PrivacyUrl);
    public static Uri TermsUri { get; } = new(TermsUrl);
}
