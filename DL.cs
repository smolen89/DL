using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using UnityEngine;

/*
 todo: dodać do pliku
private string GetVersionInfo()
		{
			return new StringBuilder().Append("Version ").Append(Application.version).Append(" - ")
				.Append(File.GetCreationTime(base.GetType().Assembly.Location))
				.Append("\n\n")
				.ToString();
		}

*/

/// <summary>
/// Klasa DL zawiera narzędzia do logowania i debugowania aplikacji.
/// </summary>
public static partial class DL
{
    static DL()
    {
        Application.logMessageReceived += Application_logMessageReceived;
        Application.quitting += Application_quitting;

        mainThread = Thread.CurrentThread;
        unityContext = SynchronizationContext.Current ?? new SynchronizationContext();

        logQueue = new Queue<string>();

        Settings = GetDefaultSettings();

        timer = new Timer((e) => SaveToFile(), null, Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Ustawienia dla logowania i debugowania.
    /// </summary>
    /// <param name="settings">Ustawienia logowania.</param>
    /// <param name="logFilePath">Niestandardowa ścieżka do pliku logów.</param>
    public static void Initialize(DLSettings? settings = null, string logFilePath = "")
    {
        logQueue.Clear();
        Settings = settings ?? GetDefaultSettings();
        InternalInitializePath(logFilePath);

        // Usunięcie nadmiarowych plików logów
        RemoveExcessLogFiles();

        // Uruchomienie timera do zapisywania logów do pliku
        // Jeśli buforowanie jest wyłączone, nie uruchamiaj timera.
        if (Settings.Buffer_Enabled)
        {
            timer?.Change(Settings.Buffer_Timeout * 1000, Settings.Buffer_Timeout * 1000);
        }
    }

    /// <summary>
    /// Inicjalizuje ścieżkę do pliku logów.
    /// </summary>
    private static void InternalInitializePath(string logFilePath)
    {
        // Ustawienie niestandardowej ścieżki do pliku logów
        filePath = string.IsNullOrEmpty(logFilePath) ? Application.persistentDataPath + $@"\{GetCurrentTimeForFileName()}.log" : logFilePath;
    }

    /// <summary>
    /// Metoda wywoływana podczas zamykania aplikacji.
    /// Używana do wykonania czyszczenia lub zapisania logów przed zakończeniem działania.
    /// </summary>
    private static void Application_quitting()
    {
        // Jeśli debugowanie jest wyłączone, nie wykonuj żadnych operacji.
        if (!enabled)
            return;

        // Jeśli zapisywanie logów jet wyłączone, nie wykonuj żadnych operacji.
        if (!Settings.All_SaveToFile)
            return;

        SaveToFile();

        if (timer == null)
            return;

        timer.Dispose();
    }

    /// <summary>
    /// Metoda rejestrująca wiadomości logów w aplikacji.
    /// Używana do przechwytywania logów z Unity i przekazywania ich do kolejki logów.
    /// </summary>
    /// <param name="condition">Warunek logowania.</param>
    /// <param name="stackTrace">Ślad stosu.</param>
    /// <param name="type">Typ logu.</param>
    private static void Application_logMessageReceived(string condition, string stackTrace, LogType type)
    {
        // Ze względu że jest to metoda wywołana przez Unity,
        // prawdopodobnie będzie to wywołane albo z błędu, albo z logu który i tak już został obsłużony
        // przez DL i nie ma sensu go ponownie obsługiwać.

        // Jeśli debugowanie jest wyłączone, nie wykonuj żadnych operacji.
        if (!enabled)
            return;

        // Jeśli zapisywanie logów jet wyłączone, nie wykonuj żadnych operacji.
        if (!Settings.All_SaveToFile)
            return;

        ProcessUnityExceptionLog(condition, stackTrace, type);
    }

    /// <summary>
    /// Wysyła wpis logu z Unity do kolejki logów.
    /// Obsługuje tylko logi typu Exception, aby uniknąć duplikacji wpisów w pliku.
    /// </summary>
    /// <param name="condition">Warunek logowania.</param>
    /// <param name="stackTrace">Ślad stosu.</param>
    /// <param name="type">Typ logu.</param>
    private static void ProcessUnityExceptionLog(string condition, string stackTrace, LogType type)
    {
        // Ze względu, że kolorowanie konsoli zapisuje się też do pliku, oraz to dubluje wpisy w pliku
        // to LogEntry z Unity będzie przejmował tylko Exceptions
        if (type != LogType.Exception)
        {
            // Jeśli to nie jest wyjątek, nie zapisuj go do pliku.
            return;
        }

        var fileQueue = new Queue<string>();

        var conditionLines = condition.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (conditionLines.Length > 1)
        {
            // zaczynamy od drugiej linijki żeby dodać odpowiednie wcięcie
            for (int i = 1; i < conditionLines.Length; i++)
            {
                var sb = new StringBuilder();
                sb.Append(string.Empty.SetLength(logEntryLeftMargin)); // Wcięcie do pierwszej linii
                sb.Append(separatorShort);
                sb.Append(conditionLines[i]);
                conditionLines[i] = sb.ToString(); // Zastąpienie linii w tablicy
            }
        }

        condition = string.Join(Environment.NewLine, conditionLines);


        // Ustawienie parametrów loga
        var logMessageBuilder = new StringBuilder();
        logMessageBuilder.Append(GetCurrentTime().SetLength(logEntryTimeLength));
        logMessageBuilder.Append(separatorShort);

        logMessageBuilder.Append(type.ToString().SetLength(logEntryTypeLength));
        logMessageBuilder.Append(separatorShort);

        logMessageBuilder.Append("Unity Runtime".SetLength(logEntryTitleLength));
        logMessageBuilder.Append(separatorShort);

        logMessageBuilder.Append(condition);

        // Dodanie loga do listy logów
        fileQueue.Enqueue(logMessageBuilder.ToString());

        // Informacja o Thread
        EnqueueThreadInformation(fileQueue.Enqueue);

        // Dodanie śladu stosu do logu
        if (Settings.StackTrace_FromUnity && Settings.StackTrace_SaveToFile)
            fileQueue.Enqueue(GenerateStackTraceLog(stackTrace));

        lock (lockLogQueue)
        {
            while (fileQueue.Count > 0)
            {
                // Pobranie logu z kolejki i dodanie go do głównej kolejki logów
                if (fileQueue.TryDequeue(out string log))
                {
                    AddToFileQueue(log);
                }
            }
        }
    }

    /// <summary>
    /// Wysyła wpis logu bezpośrednio do kolejki logów.
    /// Używana do logowania informacji, ostrzeżeń i błędów w aplikacji.
    /// </summary>
    /// <param name="title">Tytuł logu.</param>
    /// <param name="message">Treść logu.</param>
    /// <param name="type">Typ logu.</param>
    /// <param name="stackTrace">Ślad stosu.</param>
    private static void PrepareLogForSaving(string title, string message, LoggerType type, string stackTrace = "")
    {
        var fileQueue = new Queue<string>();
        var logBuilder = new StringBuilder();

        // jeśli typ jest separatorem 
        if (type == LoggerType.Separator)
        {
            logBuilder.Append(separator);
        }
        // jeśli typ list Line
        else if (type == LoggerType.Line)
        {
            // Ustawienie tytułu logu
            // Format: [        | Unity Runtime | LogMessage]

            if (string.IsNullOrEmpty(title))
            {
                // Jeśli tytuł jest pusty, ustawiamy pełny margines
                logBuilder.Append(string.Empty.SetLength(logEntryLeftMargin));

            }
            else
            {
                // Jeśli tytuł nie jest pusty, ustawiamy margines uwzględniając tytuł
                logBuilder.Append(string.Empty.SetLength(logEntryTimeLength + logEntrySeparatorLength + logEntryTypeLength));
                logBuilder.Append(separatorShort);
                logBuilder.Append(title.SetLength(logEntryTitleLength));
            }
            logBuilder.Append(separatorShort);
            logBuilder.Append(message);
        }
        else
        {
            // Ustawienie Logu
            // Format: [12:34:56.235 | LogType | Unity Runtime | Title | Message]

            logBuilder.Append(GetCurrentTime().SetLength(logEntryTimeLength));
            logBuilder.Append(separatorShort);
            logBuilder.Append(type.ToString().SetLength(logEntryTypeLength));
            logBuilder.Append(separatorShort);

            if (!string.IsNullOrEmpty(title))
                logBuilder.Append(title.SetLength(logEntryTitleLength));
            else
                logBuilder.Append("".SetLength(logEntryTitleLength));

            logBuilder.Append(separatorShort);
            logBuilder.Append(message ?? string.Empty); // Jeśli message jest null, ustaw pusty string
        }

        // Dodanie logu do listy logów
        fileQueue.Enqueue(logBuilder.ToString());

        // Jeśli ślad stosu jest pusty, nie wykonuj żadnych operacji.
        if (!string.IsNullOrEmpty(stackTrace))
        {
            // Informacja o Thread
            EnqueueThreadInformation(fileQueue.Enqueue);

            // Dodanie śladu stosu do logu
            if (Settings.StackTrace_SaveToFile)
                fileQueue.Enqueue(GenerateStackTraceLog(stackTrace));
        }

        lock (lockLogQueue)
        {
            while (fileQueue.Count > 0)
            {
                // Pobranie logu z kolejki i dodanie go do głównej kolejki logów
                if (fileQueue.TryDequeue(out string log))
                {
                    AddToFileQueue(log);
                }
            }
        }
    }

    /// <summary>
    /// Dodaje informacje o wątku do logu.
    /// Informacje te zawierają identyfikator wątku i jego nazwę, jeśli jest ustawiona.
    /// </summary>
    /// <param name="addToFileQueue">Funkcja do dodawania logów do kolejki plików.</param>
    private static void EnqueueThreadInformation(Action<string> addToFileQueue)
    {
        if (!Settings.StackTrace_ThreadInfo)
            return;

        var threadBuilder = new StringBuilder();
        threadBuilder.Append(string.Empty.SetLength(logEntryLeftMargin));
        threadBuilder.Append(separatorShort);
        threadBuilder.Append(string.Empty.SetLength(logEntryStackTraceMargin));

        threadBuilder.Append($"[Thread ID | ");
        threadBuilder.Append(Environment.CurrentManagedThreadId.ToString("D4"));
        if (!string.IsNullOrEmpty(Thread.CurrentThread.Name))
        {
            threadBuilder.Append(": ");
            threadBuilder.Append(Thread.CurrentThread.Name);
        }
        threadBuilder.Append("]");

        addToFileQueue?.Invoke(threadBuilder.ToString());
    }

    /// <summary>
    /// Formatuje ślad stosu, aby był czytelny i zawierał tylko istotne informacje.
    /// </summary>
    private static string GenerateStackTraceLog(string stackTrace)
    {
        // stwórz tablicę linii ze śladu stosu
        string[] stackLines = stackTrace.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // Jeśli ślad stosu jest pusty, nie wykonuj żadnych operacji.
        if (stackLines.Length == 0)
            return string.Empty;

        // Pominięcie linii, które nie są istotne dla debugowania
        stackLines = stackLines
            .Where(line => !line.StartsWith("UnityEngine.Debug:")               // Ignorowanie linii zaczynających się od "UnityEngine.Debug:"
                        && !line.StartsWith("UnityEngine.StackTraceUtility:")   // Ignorowanie linii zaczynających się od "UnityEngine.StackTraceUtility:"
                        && !line.StartsWith("UnityEngine.Application:")         // Ignorowanie linii zaczynających się od "UnityEngine.Application:"
                        && !line.StartsWith("UnityEditor")                      // Ignorowanie linii zaczynających się od "UnityEditor"
                        && !line.StartsWith("UnityEditor")                      // Ignorowanie linii zaczynających się od "UnityEditor"
                        && !line.StartsWith("UnityEditor")                      // Ignorowanie linii zaczynających się od "UnityEditor"
                        && !line.StartsWith("System.Threading.ThreadHelper:")                      // Ignorowanie linii zaczynających się od "UnityEditor"
                        && !line.StartsWith("System.Threading.ExecutionContext:")                      // Ignorowanie linii zaczynających się od "UnityEditor"
                        && !line.StartsWith("DL"))                              // Ignorowanie linii zaczynających się od "DL"
            .ToArray();

        // rozdzielenie linii na części, aby uzyskać nazwę metody i ścieżkę do pliku
        // i zapisanie ich z formacie "NazwaMetody (at ŚcieżkaDoPliku)"
        // Trzeba dodać do drugiej lini (ścieżka do pliku) margines, aby była czytelna
        // Pamiętając że pierwsza linia nie ma jeszcze tego wcięcia
        for (int i = 0; i < stackLines.Length; i++)
        {
            string[] parts = stackLines[i].Split(new string[] { " (at " }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                // Pierwsza część to nazwa metody, druga to ścieżka do pliku i numer linii
                string methodName = parts[0];
                // string filePath = parts[1].Trim().TrimEnd(')');
                string filePath = parts[1].Replace(")", "").Replace("]", "");

                string leftMargin = string.Empty.SetLength(logEntryLeftMargin);

                // Zastąp linię w tablicy tylko nazwą metody i ścieżką do pliku
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.Append(leftMargin); // Wcięcie do pierwszej linii
                sb.Append(separatorShort);
                sb.Append(string.Empty.SetLength(logEntryStackTraceMargin));

                sb.AppendLine(methodName);
                sb.Append(leftMargin); // Wcięcie do pierwszej linii
                sb.Append(separatorShort);
                sb.Append(string.Empty.SetLength(logEntryStackTraceMargin));
                sb.Append(filePath);

                stackLines[i] = sb.ToString(); // Zastąpienie linii w tablicy
            }
        }

        // teraz można robić wcięcie do pierwszej linii
        // i dodać separator do każdej linii, aby była czytelna
        for (int i = 0; i < stackLines.Length; i++)
        {
            var sb = new StringBuilder();
            sb.Append(string.Empty.SetLength(logEntryLeftMargin)); // Wcięcie do pierwszej linii
            sb.Append(separatorShort);
            sb.Append(string.Empty.SetLength(logEntryStackTraceMargin)); // Wcięcie dla StackTrace
            sb.Append(stackLines[i]); // Dodanie linii ze śladem stosu
            stackLines[i] = sb.ToString(); // Zastąpienie linii w tablicy
        }

        //dodanie separatora do ostatniej linii przerwy
        var lastLine = new StringBuilder();
        lastLine.Append(string.Empty.SetLength(logEntryLeftMargin)); // Wcięcie do pierwszej linii
        lastLine.Append(separatorShort);

        stackLines = stackLines.Append(lastLine.ToString()).ToArray(); // Dodanie ostatniej linii przerwy

        // Połączenie linii w jeden string z nowymi liniami
        return string.Join(Environment.NewLine, stackLines);
    }

    /// <summary>
    /// Formatuje ślad stosu dla Unity, aby był czytelny i zawierał tylko istotne informacje.
    /// </summary>
    private static string FormatStackTraceForUnity(string stackTrace)
    {
        // stwórz tablicę linii ze śladu stosu
        string[] stackLines = stackTrace.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // Jeśli ślad stosu jest pusty, nie wykonuj żadnych operacji.
        if (stackLines.Length == 0)
            return string.Empty;

        // Pominięcie linii, które nie są istotne dla debugowania
        stackLines = stackLines
            .Where(line => !line.StartsWith("UnityEngine.Debug:")               // Ignorowanie linii zaczynających się od "UnityEngine.Debug:"
                        && !line.StartsWith("UnityEngine.StackTraceUtility:")   // Ignorowanie linii zaczynających się od "UnityEngine.StackTraceUtility:"
                        && !line.StartsWith("UnityEngine.Application:")         // Ignorowanie linii zaczynających się od "UnityEngine.Application:"
                        && !line.StartsWith("UnityEditor")                      // Ignorowanie linii zaczynających się od "UnityEditor"
                        && !line.StartsWith("DL"))                              // Ignorowanie linii zaczynających się od "DL"
            .ToArray();

        // rozdzielenie linii na części, aby uzyskać nazwę metody i ścieżkę do pliku
        // i zapisanie ich z formacie "NazwaMetody (at ŚcieżkaDoPliku)"
        // Trzeba dodać do drugiej lini (ścieżka do pliku) margines, aby była czytelna
        // Pamiętając że pierwsza linia nie ma jeszcze tego wcięcia
        for (int i = 0; i < stackLines.Length; i++)
        {
            string[] parts = stackLines[i].Split(new string[] { " (at " }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                // Pierwsza część to nazwa metody, druga to ścieżka do pliku i numer linii
                string methodName = parts[0];
                // string filePath = parts[1].Trim().TrimEnd(')');
                string filePath = parts[1].Replace(")", "").Replace("]", "");

                string leftMargin = string.Empty.SetLength(logEntryLeftMargin);

                // Zastąp linię w tablicy tylko nazwą metody i ścieżką do pliku
                var sb = new StringBuilder();
                sb.AppendLine(methodName);
                sb.Append(leftMargin);
                sb.Append(separatorShort);
                sb.AppendLine(filePath);
                sb.Append(leftMargin);
                sb.Append(separatorShort);
            }
        }

        // teraz można robić wcięcie do pierwszej linii
        // i dodać separator do każdej linii, aby była czytelna
        for (int i = 0; i < stackLines.Length; i++)
        {
            var sb = new StringBuilder();
            sb.Append(string.Empty.SetLength(logEntryStackTraceMargin));
            sb.Append(stackLines[i]); // Dodanie linii ze śladem stosu
            stackLines[i] = sb.ToString(); // Zastąpienie linii w tablicy
        }

        // Połączenie linii w jeden string z nowymi liniami
        return string.Join(Environment.NewLine, stackLines);
    }

    /// <summary>
    /// Dodaje wpis do kolejki logów i sprawdza, czy należy zapisać bufor do pliku.
    /// </summary>
    /// <param name="log">Wpis logu do dodania.</param>
    private static void AddToFileQueue(string log)
    {
        // ConcurrentQueue jest bezpieczna wątkowo, więc nie trzeba używać locka przy Enqueue.
        logQueue.Enqueue(log);
        bufferIndex++;
        CheckBufferSaveFile();
    }

    /// <summary>
    /// Sprawdza, czy bufor logów osiągnął maksymalny rozmiar i zapisuje go do pliku, jeśli to konieczne.
    /// </summary>
    private static void CheckBufferSaveFile()
    {
        // Sprawdzenie czy jest kolejka logów
        if (logQueue.Count == 0)
            return;

        // Jeśli buforowanie jest wyłączone, nie wykonuj żadnych operacji.
        if (!Settings.Buffer_Enabled)
            return;

        // Sprawdzenie czy bufor osiągnął maksymalny rozmiar
        if (bufferIndex >= Settings.Buffer_Size)
        {
            SaveToFile();
        }
    }

    /// <summary>
    /// Dopisuje logi z kolejki do pliku.
    /// </summary>
    private static void SaveToFile()
    {
        // Sprawdzenie czy jest kolejka logów
        if (logQueue.Count == 0)
            return;

        // Jeśli zapisywanie logów jest wyłączone, nie wykonuj żadnych operacji.
        if (!Settings.All_SaveToFile)
            return;

        InternalSaveToFile();
    }

    private static void BufferedSaveToFile()
    {
        // Jeśli buforowanie jest wyłączone, nie wykonuj żadnych operacji.
        if (!Settings.Buffer_Enabled)
            return;

        // Wymuszenie zapisu logów do pliku
        SaveToFile();
    }

    /// <summary>
    /// Wymusza zapis logów do pliku.
    /// </summary>
    public static void ForceSaveToFile()
    {
        InternalSaveToFile();
    }

    private static void InternalSaveToFile()
    {
        lock (lockFile)
        {
            // Sprawdzenie czy jest kolejka logów
            if (logQueue.Count == 0)
                return;

            var logLines = new List<string>();

            // Pobranie z kolejki logów danych do zapisania, tak by nie przeszkadzało w wielowątkowości
            while (logQueue.Count > 0)
            {
                if (logQueue.TryDequeue(out string logEntry))
                {
                    logLines.Add(logEntry);
                }
            }

            // Sprawdzenie czy jest ustawiona ścieżka do pliku, nie wiem czemu ale czasem gubi ścieżkę,
            // więc trzeba ją zainicjalizować ponownie, może dlatego że W UnityEditor nie ma wywołania DL.Initialize()
            // i wtedy filePath jest pusty, a w buildzie jest ustawiona ścieżka.
            // W buildzie jest ustawiona ścieżka w KernelInitializer.cs
            if (string.IsNullOrEmpty(filePath))
            {
                InternalInitializePath(filePath);
            }

            // Zapis do pliku
            File.AppendAllLines(filePath, logLines, Encoding.UTF8);

            logQueue.Clear();
            bufferIndex = 0; // Resetowanie indeksu bufora po zapisaniu
        }
    }

    /// <summary>
    /// Usuwa nadmiarowe pliki logów, jeśli ich liczba przekracza dozwoloną wartość.
    /// Pozwala to na utrzymanie historii logów z poprzednich uruchomień aplikacji,
    /// zachowując jednocześnie porządek i unikając zapełnienia dysku.
    /// </summary>
    private static void RemoveExcessLogFiles()
    {
        // Pobranie wszystkich plików logów w katalogu aplikacji
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.persistentDataPath);
        FileInfo[] logFiles = directoryInfo.GetFiles("*.log", SearchOption.TopDirectoryOnly);

        // Sprawdzenie czy liczba plików z logami jest większa niż maksymalna dozwolona liczba plików
        if (logFiles.Length <= Settings.FileLogMaxCount)
            return;

        // Usunięcie najstarszych plików logów
        Array.Sort(logFiles, (f1, f2) => f1.CreationTime.CompareTo(f2.CreationTime));
        for (int i = 0; i < logFiles.Length - Settings.FileLogMaxCount; i++)
        {
            logFiles[i].Delete();
        }

        /* Czytelniejsze zostawia to w komentarzu, ale można to odkomentować jeśli potrzebne
        // Posortowanie plików według daty utworzenia i usunięcie nadmiarowych plików
        var filesToRemove = logFiles
            .OrderBy(file => file.CreationTime)
            .Take(logFiles.Length - Settings.FileLogMaxCount);

        foreach (var file in filesToRemove)
        {
            file.Delete();
        }
        */
    }

    /// <summary>
    /// Czyści listę logów, resetując indeks bufora i usuwając wszystkie wpisy z kolejki.
    /// </summary>
    public static void ClearLogList()
    {
        if (logQueue.Count == 0)
            return;

        // Jeśli debugowanie jest wyłączone, nie wykonuj żadnych operacji.
        if (!enabled)
            return;

        logQueue.Clear();
        bufferIndex = 0; // Resetowanie indeksu bufora po czyszczeniu
    }

    public static string Information()
    {
        return AppInfo.FullNameApplication;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InternalLog(string message)
    {
        // Zmiana wyświetlania StackTrace dla unity
        var stackTraceState = Application.GetStackTraceLogType(LogType.Log);
        Application.SetStackTraceLogType(LogType.Log, Settings.StackTrace_ShowInUnity ? StackTraceLogType.ScriptOnly : StackTraceLogType.None);
        Debug.Log(message);
        // Przywrócenie stanu StackTrace
        Application.SetStackTraceLogType(LogType.Log, stackTraceState);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InternalLine(string message)
    {
        // Zmiana wyświetlania StackTrace dla unity
        var stackTraceState = Application.GetStackTraceLogType(LogType.Log);
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Debug.Log(message);
        // Przywrócenie stanu StackTrace
        Application.SetStackTraceLogType(LogType.Log, stackTraceState);
    }

}
