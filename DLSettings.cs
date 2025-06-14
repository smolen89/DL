public static partial class DL
{
    /// <summary>
    /// Struktura DLSettings zawiera ustawienia konfiguracyjne dla logowania i debugowania w aplikacji.
    /// </summary>
    public class DLSettings
    {


        /// <summary>
        /// Ustawienie bufora zapisu logów.
        /// Jeśli jest ustawione na true, logi będą buforowane i zapisywane do pliku po osiągnięciu określonego czasu.
        /// </summary>
        public bool Buffer_Enabled { get; set; }

        /// <summary>
        /// Rozmiar bufora w wpisach.
        /// Jeśli ilość wpisów w buforze przekroczy ten rozmiar, zostanie on zapisany do pliku.
        /// </summary>
        public int Buffer_Size { get; set; }

        /// <summary>
        /// Czas oczekiwania na zapis bufora w sekundach.
        /// </summary>
        public int Buffer_Timeout { get; set; }

        /// <summary>
        /// Maksymalna liczba plików logów, które będą przechowywane.
        /// </summary>
        public int FileLogMaxCount { get; set; }

        /// <summary>
        /// Ustawienie, czy aplikacja jest w trybie debugowania.
        /// </summary>
        public bool IsDebugBuild { get; set; }

        /// <summary>
        /// Ustawienie, czy logowanie do Unity Debug jest włączone.
        /// </summary>
        public bool All_ShowInUnity { get; set; }

        /// <summary>
        /// Ustawienie, czy logi debugowania powinny być zapisywane do pliku.
        /// </summary>
        public bool All_SaveToFile { get; set; }

        /// <summary>
        /// Ustawienie, czy StackTrace powinien być pobierany z Unity.
        /// </summary>
        public bool StackTrace_FromUnity { get; set; }

        /// <summary>
        /// Ustawienie, czy StackTrace powinien być zapisywany do pliku.
        /// </summary>
        public bool StackTrace_SaveToFile { get; set; }

        /// <summary>
        /// Ustawienie, czy StackTrace powinien być wyświetlany w konsoli Unity.
        /// </summary>
        public bool StackTrace_ShowInUnity { get; set; }

        /// <summary>
        /// Ustawienie, czy StackTrace powinien zawierać informacje o wątkach.
        /// Jeśli jest ustawione na true, StackTrace będzie zawierał informacje o wątkach.
        /// </summary>
        public bool StackTrace_ThreadInfo { get; set; }

        /// <summary>
        /// Ustawienie, czy linie debugowania powinny być wyświetlane w konsoli Unity.
        /// </summary>
        public bool Line_ShowInUnity { get; set; }

        /// <summary>
        /// Ustawienie, czy linie debugowania powinny być zapisywane do pliku.
        /// </summary>
        public bool Line_SaveToFile { get; set; }

        /// <summary>
        /// Ustawienie, czy logi powinny być wyświetlane w konsoli Unity.
        /// </summary>
        public bool Log_ShowInUnity { get; set; }

        /// <summary>
        /// Ustawienie, czy logi powinny być zapisywane do pliku.
        /// </summary>
        public bool Log_SaveToFile { get; set; }

        /// <summary>
        /// Ustawienie, czy StackTrace powinien być zapisywany do pliku.
        /// </summary>
        public bool StackTrace_Log_SaveToFile { get; set; }

        /// <summary>
        /// Ustawienie, czy logi informacyjne powinny być wyświetlane w konsoli Unity.
        /// </summary>
        public bool LogInfo_ShowInUnity { get; set; }

        /// <summary>
        /// Ustawienie, czy logi informacyjne powinny być zapisywane do pliku.
        /// </summary>
        public bool LogInfo_SaveToFile { get; set; }

        /// <summary>
        /// Ustawienie, czy StackTrace powinien być wyświetlany w konsoli Unity.
        /// </summary>
        public bool StackTrace_LogInfo_SaveToFile { get; set; }

        /// <summary>
        /// Ustawienie, czy logi ostrzeżeń powinny być zapisywane do pliku.
        /// </summary>
        public bool LogWarning_SaveToFile { get; set; }

        /// <summary>
        /// Ustawienie, czy logi ostrzeżeń powinny być wyświetlane w konsoli Unity.
        /// </summary>
        public bool LogWarning_ShowInUnity { get; set; }

        /// <summary>
        /// Ustawienie, czy StackTrace powinien być zapisywany do pliku dla logów ostrzeżeń.
        /// </summary>
        public bool StackTrace_LogWarning_SaveToFile { get; set; }

        /// <summary>
        /// Ustawienie, czy logi błędów powinny być zapisywane do pliku.
        /// </summary>
        public bool LogError_SaveToFile { get; set; }

        /// <summary>
        /// Ustawienie, czy logi błędów powinny być wyświetlane w konsoli Unity.
        /// </summary>
        public bool LogError_ShowInUnity { get; set; }

        /// <summary>
        /// Ustawienie, czy StackTrace powinien być zapisywany do pliku dla logów błędów.
        /// </summary>
        public bool StackTrace_LogError_SaveToFile { get; set; }
    }
}