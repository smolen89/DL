using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;


public partial class DL
{
    /// <summary>
    /// Struktura DateTimeFormat zawiera predefiniowane formaty daty i czasu do użycia w aplikacji.
    /// </summary>
    public struct DateTimeFormat
    {
        /// <summary>
        /// Format daty i czasu w formacie: 2020-01-01 12:00:00.000
        /// </summary>
        public const string Full = "yyyy-MM-dd HH:mm:ss.fff";

        /// <summary>
        /// Format daty i czasu w formacie: 2020-01-01 12.00.00.000
        /// </summary>
        public const string File = "yyyy-MM-dd HH.mm.ss.fff";
        /// <summary>
        /// Format daty i czasu w formacie: 2020-01-01 12:00:00
        /// </summary>
        public const string Long = "yyyy-MM-dd HH:mm:ss";
        /// <summary>
        /// Format daty i czasu w formacie: 2020-01-01
        /// </summary>
        public const string Short = "yyyy-MM-dd";
        /// <summary>
        /// Format daty i czasu w formacie: 12:00:00.000
        /// </summary>
        public const string Time = "HH:mm:ss.fff";
        /// <summary>
        /// Format daty i czasu w formacie: 12:00:00
        /// </summary>
        public const string TimeShort = "HH:mm:ss";
    }


    private static Timer timer;

    private const string separatorBold = "==================================================================================================================";
    private const string separator = "------------------------------------------------------------------------------------------------------------------";
    private const string separatorShort = " | ";

    /// <summary>
    /// Kolejka do logowania.
    /// </summary>
    private static readonly Queue<string> logQueue = new Queue<string>();

    /// <summary>
    /// Zwraca obecny czas w formacie "HH:mm:ss.fff".
    /// Używany do logowania i formatowania tekstu.
    /// </summary>
    private static string GetCurrentTime() => DateTime.Now.ToString(DateTimeFormat.Time);

    /// <summary>
    /// Zwraca obecny czas w formacie "yyyy-MM-dd HH.mm.ss.fff".
    /// Używany do tworzenia nazw plików logów.
    /// </summary>
    private static string GetCurrentTimeForFileName() => DateTime.Now.ToString(DateTimeFormat.File);

    /// <summary>
    /// Czy debuger jest aktywny, jest to możliwość wyłączenia całkowicie debugowania i logowania.
    /// </summary>
    private static bool enabled = true;

    /// <summary>
    /// Ścieżka do pliku logu.
    /// </summary>
    private static string filePath = string.Empty;

    /// <summary>
    /// Obiekt blokujący dostęp do pliku logu.
    /// Używany do synchronizacji dostępu do pliku logu, aby uniknąć konfliktów podczas zapisu z wielu wątków.
    /// </summary>
    private static readonly object lockFile = new object();

    private static readonly object lockLogQueue = new object();

    /// <summary>
    /// Wątek główny aplikacji.
    /// Używany do synchronizacji kontekstu Unity, aby zapewnić, że logowanie odbywa się w głównym wątku.
    /// </summary>
    private static readonly Thread mainThread;

    /// <summary>
    /// Kontekst synchronizacji Unity.
    /// Używany do synchronizacji logowania w kontekście Unity, aby uniknąć problemów z wielowątkowością.
    /// </summary>
    private static readonly SynchronizationContext unityContext;

    /// <summary>
    /// Indeks bufora logów.
    /// Używany do przechowywania logów w buforze przed ich zapisaniem do pliku lub wyświetleniem.
    /// </summary>
    private static int bufferIndex = 0;

    /// <summary>
    /// Długość czasu w logach.
    /// Używana do formatowania czasu w logach, np. "HH:mm:ss.fff" ma 12 znaków.
    /// </summary>
    private const int logEntryTimeLength = 12;

    /// <summary>
    /// Długość typu wpisu w logach.
    /// Używana do formatowania typu wpisu w logach, np. "ERROR" ma 10 znaków.
    /// </summary>
    private const int logEntryTypeLength = 10;

    /// <summary>
    /// Długość tytułu wpisu w logach.
    /// Używana do formatowania tytułu wpisu w logach, np. "MyLogEntry" ma 30 znaków.
    /// </summary>
    private const int logEntryTitleLength = 30;

    /// <summary>
    /// Długość separatora wpisu w logach.
    /// Używana do formatowania separatora wpisu w logach, np. " | " ma 3 znaki.
    /// </summary>
    private const int logEntrySeparatorLength = 3;

    /// <summary>
    /// Długość lewego marginesu wpisu w logach.
    /// Używana do formatowania lewego marginesu wpisu w logach, aby wyrównać tekst.
    /// </summary>
    private const int logEntryLeftMargin = logEntryTimeLength + logEntryTypeLength + logEntryTitleLength + (logEntrySeparatorLength * 2);

    /// <summary>
    /// Długość prawego marginesu wpisu w logach.
    /// Używana do formatowania prawego marginesu wpisu w logach, aby wyrównać tekst.
    /// </summary>
    private const int logEntryStackTraceMargin = 4;

    #region Kolory i formatowanie
    /// <summary>
    /// Kolor linii w logach.
    /// </summary>
    private static readonly string lineColor = Colors.Gray;

    /// <summary>
    /// Kolor tekstu dla argumentów w logach.
    /// </summary>
    private static readonly string argsColor = Colors.Cyan;

    /// <summary>
    /// Kolor tekstu w logach.
    /// </summary>
    private static readonly string defaultColor = Colors.White;

    /// <summary>
    /// Kolor logowania błędów.
    /// </summary>
    private static readonly string errorColor = Colors.Red;

    /// <summary>
    /// Kolor logowania ostrzeżeń.
    /// </summary>
    private static readonly string warningColor = Colors.Yellow;

    /// <summary>
    /// Kolor logowania informacji.
    /// </summary>
    private static readonly string infoColor = Colors.Lime;

    #endregion

    #region String Foramatting
    /// <summary>
    /// Zwraca tekst otoczony tagiem color do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tekst wejściowy.</param>
    /// <param name="color">Kolor w formacie hex (np. "#ff0000ff").</param>
    /// <returns>Tekst z tagiem color o podanym kolorze.</returns>
    private static string Colored(this string source, string color)
    {
        var sb = new StringBuilder();
        sb.Append("<color=");
        sb.Append(color);
        sb.Append(">");
        sb.Append(source);
        sb.Append("</color>");
        return sb.ToString();
    }

    /// <summary>
    /// Zwraca tablicę tekstów otoczonych tagiem color do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tablica tekstów wejściowych.</param>
    /// <param name="color">Kolor w formacie hex (np. "#ff0000ff").</param>
    /// <returns>Tablica tekstów z tagiem color o podanym kolorze.</returns>
    private static string[] Colored(this string[] source, string color)
    {
        string[] temp = new string[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            temp[i] = source[i].Colored(color);
        }
        return temp;
    }

    /// <summary>
    /// Zwraca tekst otoczony tagiem <size=...> do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tekst wejściowy.</param>
    /// <param name="size">Rozmiar czcionki.</param>
    /// <returns>Tekst z tagiem <size> o podanym rozmiarze.</returns>
    private static string Sized(this string source, int size)
    {
        var sb = new StringBuilder();
        sb.Append("<size=");
        sb.Append(size);
        sb.Append(">");
        sb.Append(source);
        sb.Append("</size>");
        return sb.ToString();
    }

    /// <summary>
    /// Zwraca tablicę tekstów otoczonych tagiem size do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tablica tekstów wejściowych.</param>
    /// <param name="size">Rozmiar czcionki.</param>
    /// <returns>Tablica tekstów z tagiem size o podanym rozmiarze.</returns>
    private static string[] Sized(this string[] source, int size)
    {
        string[] temp = new string[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            temp[i] = source[i].Sized(size);
        }
        return temp;
    }

    /// <summary>
    /// Zwraca tekst otoczony tagiem bold do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tekst wejściowy.</param>
    /// <returns>Tekst z tagiem bold (pogrubienie).</returns>
    private static string Bold(this string source)
    {
        var sb = new StringBuilder();
        sb.Append("<b>");
        sb.Append(source);
        sb.Append("</b>");
        return sb.ToString();
    }

    /// <summary>
    /// Zwraca tablicę tekstów otoczonych tagiem bold do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tablica tekstów wejściowych.</param>
    /// <returns>Tablica tekstów z tagiem bold (pogrubienie).</returns>
    private static string[] Bold(this string[] source)
    {
        string[] temp = new string[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            temp[i] = source[i].Bold();
        }
        return temp;
    }

    /// <summary>
    /// Zwraca tekst otoczony tagiem italic do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tekst wejściowy.</param>
    /// <returns>Tekst z tagiem italic (kursywa).</returns>
    private static string Italic(this string source)
    {
        var sb = new StringBuilder();
        sb.Append("<i>");
        sb.Append(source);
        sb.Append("</i>");
        return sb.ToString();
    }

    /// <summary>
    /// Zwraca tablicę tekstów otoczonych tagiem italic do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tablica tekstów wejściowych.</param>
    /// <returns>Tablica tekstów z tagiem italic (kursywa).</returns>
    private static string[] Italic(this string[] source)
    {
        string[] temp = new string[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            temp[i] = source[i].Italic();
        }
        return temp;
    }

    /// <summary>
    /// Zwraca tekst otoczony tagiem underline do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tekst wejściowy.</param>
    /// <returns>Tekst z tagiem underline (podkreślenie).</returns>
    private static string Underline(this string source)
    {
        var sb = new StringBuilder();
        sb.Append("<u>");
        sb.Append(source);
        sb.Append("</u>");
        return sb.ToString();
    }

    /// <summary>
    /// Zwraca tablicę tekstów otoczonych tagiem underline do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tablica tekstów wejściowych.</param>
    /// <returns>Tablica tekstów z tagiem underline (podkreślenie).</returns>
    private static string[] Underline(this string[] source)
    {
        string[] temp = new string[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            temp[i] = source[i].Underline();
        }
        return temp;
    }

    /// <summary>
    /// Zwraca tekst otoczony tagiem strikethrough do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tekst wejściowy.</param>
    /// <returns>Tekst z tagiem strikethrough (przekreślenie).</returns>
    private static string Strikethrough(this string source)
    {
        var sb = new StringBuilder();
        sb.Append("<s>");
        sb.Append(source);
        sb.Append("</s>");
        return sb.ToString();
    }

    /// <summary>
    /// Zwraca tablicę tekstów otoczonych tagiem strikethrough do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tablica tekstów wejściowych.</param>
    /// <returns>Tablica tekstów z tagiem strikethrough (przekreślenie).</returns>
    private static string[] Strikethrough(this string[] source)
    {
        string[] temp = new string[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            temp[i] = source[i].Strikethrough();
        }
        return temp;
    }

    /// <summary>
    /// Zwraca tekst otoczony tagiem link do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tekst wejściowy.</param>
    /// <param name="url">URL docelowy linku.</param>
    /// <returns>Tekst z tagiem link wskazującym na podany URL.</returns>
    private static string Link(this string source, string url)
    {
        var sb = new StringBuilder();
        sb.Append("<link=");
        sb.Append(url);
        sb.Append(">");
        sb.Append(source);
        sb.Append("</link>");
        return sb.ToString();
    }

    /// <summary>
    /// Zwraca tablicę tekstów otoczonych tagiem link do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tablica tekstów wejściowych.</param>
    /// <param name="url">URL docelowy linku.</param>
    /// <returns>Tablica tekstów z tagiem link wskazującym na podany URL.</returns>
    private static string[] Link(this string[] source, string url)
    {
        string[] temp = new string[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            temp[i] = source[i].Link(url);
        }
        return temp;
    }

    /// <summary>
    /// Zwraca tekst otoczony tagiem quote do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tekst wejściowy.</param>
    /// <returns>Tekst z tagiem quote (cytat).</returns>
    private static string Quote(this string source)
    {
        var sb = new StringBuilder();
        sb.Append("<quote>");
        sb.Append(source);
        sb.Append("</quote>");
        return sb.ToString();
    }

    /// <summary>
    /// Zwraca tablicę tekstów otoczonych tagiem quote do użycia w Unity UI.
    /// </summary>
    /// <param name="source">Tablica tekstów wejściowych.</param>
    /// <returns>Tablica tekstów z tagiem quote (cytat).</returns>
    private static string[] Quote(this string[] source)
    {
        string[] temp = new string[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            temp[i] = source[i].Quote();
        }
        return temp;
    }

    /// <summary>
    /// Ustawia długość tekstu do zadanej wartości. Jeśli tekst jest dłuższy, może zostać obcięty (truncate=true).
    /// Jeśli krótszy, zostanie dopełniony spacjami.
    /// </summary>
    /// <param name="source">Tekst wejściowy.</param>
    /// <param name="length">Docelowa długość.</param>
    /// <param name="truncate">Czy obcinać tekst, jeśli jest za długi.</param>
    /// <returns>Tekst o zadanej długości.</returns>
    private static string SetLength(this string source, int length, bool truncate = false)
    {
        if (source.Length > length)
        {
            return truncate ? source[..length] : source;
        }
        else if (source.Length < length)
        {
            // StringBuilder jest bardziej wydajny przy dopełnianiu tekstu

            // do ustalonej długości, ponieważ unika tworzenia wielu nowych instancji stringów.
            StringBuilder sb = new StringBuilder(source);
            sb.Append(' ', length - source.Length);
            return sb.ToString();
        }
        else
        {
            return source;
        }
    }

    /// <summary>
    /// Zwraca tablicę tekstów o ustalonej długości do zadanej wartości. 
    /// Jeśli tekst jest dłuższy, może zostać obcięty (truncate=true).
    /// Jeśli krótszy, zostanie dopełniony spacjami.
    /// </summary>
    /// <param name="source">Tablica tekstów wejściowych.</param>
    /// <param name="length">Docelowa długość.</param>
    /// <param name="truncate">Czy obcinać tekst, jeśli jest za długi.</param>
    /// <returns>Tablica tekstów o zadanej długości.</returns>
    private static string[] SetLength(this string[] source, int length, bool truncate = false)
    {
        string[] temp = new string[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            temp[i] = source[i].SetLength(length, truncate);
        }
        return temp;
    }

    /// <summary>
    /// Struktura zawierająca predefiniowane kolory w formacie hex do użycia z tagami Unity UI.
    /// Wszystkie kolory są w formacie RGBA z pełną przezroczystością (FF na końcu).
    /// </summary>
    public struct Colors
    {
        /// <summary>Czerwony - #ff0000ff</summary>
        public const string Red = "#ff0000ff";

        /// <summary>Żółty - #ffff00ff</summary>
        public const string Yellow = "#ffff00ff";

        /// <summary>Zielony - #008000ff</summary>
        public const string Green = "#008000ff";

        /// <summary>Cyjan - #00ffffff</summary>
        public const string Cyan = "#00ffffff";

        /// <summary>Biały - #ffffffff</summary>
        public const string White = "#ffffffff";

        /// <summary>Czarny - #000000ff</summary>
        public const string Black = "#000000ff";

        /// <summary>Magenta - #ff00ffff</summary>
        public const string Magenta = "#ff00ffff";

        /// <summary>Niebieski - #0000ffff</summary>
        public const string Blue = "#0000ffff";

        /// <summary>Szary - #808080ff</summary>
        public const string Gray = "#808080ff";

        /// <summary>Srebrny - #c0c0c0ff</summary>
        public const string Silver = "#c0c0c0ff";

        /// <summary>Kasztanowy - #800000ff</summary>
        public const string Maroon = "#800000ff";

        /// <summary>Oliwkowy - #808000ff</summary>
        public const string Olive = "#808000ff";

        /// <summary>Purpurowy - #800080ff</summary>
        public const string Purple = "#800080ff";

        /// <summary>Teal - #008080ff</summary>
        public const string Teal = "#008080ff";

        /// <summary>Granatowy - #000080ff</summary>
        public const string Navy = "#000080ff";

        /// <summary>Limonkowy - #00ff00ff</summary>
        public const string Lime = "#00ff00ff";
    }
    #endregion


    public static DLSettings Settings { get; set; }

    /// <summary>
    /// Właściwość umożliwiająca włączenie lub wyłączenie debugowania.
    /// Kiedy debuger jest wyłączony, wszystkie metody debugowania są przerywane.
    /// </summary>
    public static bool Enabled
    {
        get => enabled;
        set
        {
            if (value != enabled)
            {
                enabled = value;
                if (enabled)
                {
                    // Restart the timer if it was disabled
                    timer?.Change(0, Settings.Buffer_Timeout * 1000);
                }
                else
                {
                    // Stop the timer when disabled
                    timer?.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
        }
    }

    /// <summary>
    /// Ustawia ilość plików logów.
    /// </summary>
    public static void SetFileCount(int count)
    {
        Settings.FileLogMaxCount = count;
        if (Settings.FileLogMaxCount < 1)
        {
            Settings.FileLogMaxCount = 1;
        }
        else if (Settings.FileLogMaxCount > 100)
        {
            Settings.FileLogMaxCount = 100;
        }
    }

    public static DLSettings GetDefaultSettings()
    {
        return new DLSettings
        {
            Buffer_Enabled = true,
            Buffer_Timeout = 5,
            FileLogMaxCount = 10,
            IsDebugBuild = true,

            All_SaveToFile = true,
            All_ShowInUnity = true,

            StackTrace_FromUnity = true,
            StackTrace_SaveToFile = true,
            StackTrace_ThreadInfo = true,
            StackTrace_ShowInUnity = false,

            Line_SaveToFile = true,
            Line_ShowInUnity = true,

            Log_ShowInUnity = true,
            Log_SaveToFile = true,
            StackTrace_Log_SaveToFile = true,

            LogInfo_SaveToFile = true,
            LogInfo_ShowInUnity = true,
            StackTrace_LogInfo_SaveToFile = true,

            LogWarning_SaveToFile = true,
            LogWarning_ShowInUnity = true,
            StackTrace_LogWarning_SaveToFile = true,

            LogError_ShowInUnity = true,
            LogError_SaveToFile = true,
            StackTrace_LogError_SaveToFile = true
        };
    }
}