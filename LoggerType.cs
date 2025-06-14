/// <summary>
/// Typy logów.
/// </summary>
public enum LoggerType : int
{
    Error = 0,      // Log błędu
    Info = 1,       // Log informacyjny
    Warning = 2,    // Log ostrzeżenia
    Log = 3,        // Log zwykły
    Exception = 4,  // Log wyjątku
    StackTrace = 5, // Log śladu stosu
    Line = 6,       // Log linii
    Separator = 7,  // Log separatora
}

/*   LogType Unity
public enum LogType
{
	Error,
	Assert,
	Warning,
	Log,
	Exception
}
 */