public partial class DL
{
    /// <summary>
    /// Udostępnia metadane aplikacji, takie jak wersja, data wydania oraz informacje o nazwie Debug Loggera.
    /// </summary>
    public class AppInfo
    {
        public const string Version = "v1.4";
        public const string VersionDate = "2025-06-13";
        public const string VersionInfo = "DL " + Version + " (" + VersionDate + ")";
        public const string NameApplicationInfoShort = "DL " + Version;
        public const string NameApplication = "Debug Logger";
        public const string FullNameApplication = "Debbug Logger (DL)" + Version + " (" + VersionDate + ")";
    }
}