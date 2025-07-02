using System;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

public static partial class DL
{
    /// <summary>
    /// Wywołuje Unity.Debug.Break().
    /// </summary>
    public static void Break()
    {
        if (!enabled) return;

        Debug.Break();
    }

    /// <summary>
    /// Czyści konsole Unity.
    /// </summary>
    public static void ClearDeveloperConsole()
    {
        if (!enabled) return;

        Debug.ClearDeveloperConsole();
    }

    /// <summary>
    /// Wyświetla w kosoli linie tekstu, ze względu że ogólne debugowanie jest czasochłonne
    /// ta metoda musi być szybka oraz łatwo pomijalna
    /// Najczęściej będzie używana do loppów i innych miejsc gdzie jest dużo danych do wyświetlenia.
    /// </summary>
    public static void Line(object message)
    {
        if (!enabled) return;

        var cacheMessage = message == null ? "_____" : message.ToString();

        // Zapis do pliku
        if (Settings.All_SaveToFile && Settings.Line_SaveToFile)
        {
            PrepareLogForSaving(string.Empty, cacheMessage, LoggerType.Line);
        }

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (!Settings.Line_ShowInUnity) return;

        var result = new StringBuilder(cacheMessage.Colored(lineColor));

        if (Thread.CurrentThread == mainThread)
        {
            InternalLine(result.ToString());
        }
        else
        {
            unityContext.Post(_ =>
            {
                InternalLine(result.ToString());
            }, null);
        }
    }

    /// <summary>
    /// Wyświetla w kosoli linie tekstu, ze względu że ogólne debugowanie jest czasochłonne
    /// ta metoda musi być szybka oraz łatwo pomijalna
    /// </summary>
    /// <param name="channel">Kanał logowania.</param>
    /// <param name="message">Wiadomość do zalogowania.</param>
    public static void Line(string channel, object message)
    {
        if (!enabled) return;

        var cacheMessage = message == null ? "_____" : message.ToString();

        // Zapis do pliku
        if (Settings.All_SaveToFile && Settings.Line_SaveToFile)
        {
            PrepareLogForSaving(channel, cacheMessage, LoggerType.Line);
        }

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (!Settings.Line_ShowInUnity) return;

        var result = new StringBuilder();
        result.Append(channel);
        result.Append(": ");
        result.Append(cacheMessage);

        if (Thread.CurrentThread == mainThread)
        {
            InternalLine(result.ToString().Colored(lineColor));
        }
        else
        {
            unityContext.Post(_ =>
            {
                InternalLine(result.ToString().Colored(lineColor));
            }, null);
        }
    }

    /// <summary>
    /// Wyświetla w kosoli linie tekstu, ze względu że ogólne debugowanie jest czasochłonne
    /// ta metoda musi być szybka oraz łatwo pomijalna
    /// </summary>
    /// <param name="channel">Kanał logowania.</param>
    /// <param name="message">Wiadomość do zalogowania.</param>
    /// <param name="args">Argumenty do sformatowania wiadomości.</param>
    public static void Line(string channel, object message, params object[] args)
    {
        if (!enabled) return;

        var cacheMessage = message == null ? "_____" : message.ToString();

        // Zapis do pliku
        if (Settings.All_SaveToFile && Settings.Line_SaveToFile)
        {
            PrepareLogForSaving(channel, string.Format(cacheMessage, args), LoggerType.Line);
        }

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (!Settings.Line_ShowInUnity) return;

        // Formatowanie argumentów
        var resultArgs = new StringBuilder[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            resultArgs[i] = new StringBuilder(args[i].ToString());
        }

        var result = new StringBuilder();
        result.Append(channel);
        result.Append(": ");
        result.AppendFormat(cacheMessage, resultArgs);

        if (Thread.CurrentThread == mainThread)
        {
            InternalLine(result.ToString().Colored(lineColor));
        }
        else
        {
            unityContext.Post(_ =>
            {
                InternalLine(result.ToString().Colored(lineColor));
            }, null);
        }
    }

    /// <summary>
    /// Loguje wiadomość w aplikacji.
    /// </summary>
    /// <param name="message">Wiadomość do zalogowania.</param>
    public static void Log(object message)
    {
        if (!enabled) return;

        var cacheMessage = message == null ? "_____" : message.ToString();

        // Zapis do pliku
        if (Settings.All_SaveToFile && Settings.Log_SaveToFile)
        {
            if (Settings.StackTrace_SaveToFile && Settings.StackTrace_Log_SaveToFile)
            {
                // Zapis z StackTrace
                PrepareLogForSaving("", cacheMessage, LoggerType.Line, StackTraceUtility.ExtractStackTrace());
            }
            else
            {
                // Zapis bez StackTrace
                PrepareLogForSaving("", cacheMessage, LoggerType.Line);
            }
        }

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (!Settings.Log_ShowInUnity) return;

        var result = new StringBuilder(cacheMessage.Colored(defaultColor));

        if (Thread.CurrentThread == mainThread)
        {
            InternalLog(result.ToString());
        }
        else
        {
            unityContext.Post(_ =>
            {
                InternalLog(result.ToString());
            }, null);
        }
    }

    /// <summary>
    /// Loguje wiadomość w aplikacji z określonym kanałem.
    /// </summary>
    /// <param name="channel">Kanał logowania.</param>
    /// <param name="message">Wiadomość do zalogowania.</param>
    public static void Log(string channel, object message)
    {
        if (!enabled) return;

        var cacheMessage = message == null ? "_____" : message.ToString();

        // Zapis do pliku
        if (Settings.All_SaveToFile && Settings.Log_SaveToFile)
        {
            if (Settings.StackTrace_SaveToFile && Settings.StackTrace_Log_SaveToFile)
            {
                // Zapis z StackTrace
                PrepareLogForSaving(channel, cacheMessage, LoggerType.Log, StackTraceUtility.ExtractStackTrace());
            }
            else
            {
                // Zapis bez StackTrace
                PrepareLogForSaving(channel, cacheMessage, LoggerType.Log);
            }
        }

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (!Settings.Log_ShowInUnity) return;

        var result = new StringBuilder();
        result.Append(channel.Bold());
        result.Append(": ");
        result.Append(cacheMessage);

        if (Thread.CurrentThread == mainThread)
        {
            InternalLog(result.ToString().Colored(defaultColor));
        }
        else
        {
            unityContext.Post(_ =>
            {
                InternalLog(result.ToString().Colored(defaultColor));
            }, null);
        }

    }

    /// <summary>
    /// Loguje wiadomość w aplikacji z określonym kanałem.
    /// </summary>
    /// <param name="channel">Kanał logowania.</param>
    /// <param name="message">Wiadomość do zalogowania.</param>
    /// <param name="args">Argumenty do sformatowania wiadomości.</param>
    public static void Log(string channel, object message, params object[] args)
    {
        if (!enabled) return;

        var cacheMessage = message == null ? "_____" : message.ToString();

        // Zapis do pliku
        if (Settings.All_SaveToFile && Settings.Log_SaveToFile)
        {
            if (Settings.StackTrace_SaveToFile && Settings.StackTrace_Log_SaveToFile)
            {
                // Zapis z StackTrace
                PrepareLogForSaving(channel, string.Format(cacheMessage, args), LoggerType.Log, StackTraceUtility.ExtractStackTrace());
            }
            else
            {
                // Zapis bez StackTrace
                PrepareLogForSaving(channel, string.Format(cacheMessage, args), LoggerType.Log);
            }
        }

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (!Settings.Log_ShowInUnity) return;

        // Formatowanie argumentów
        var resultArgs = new StringBuilder[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            resultArgs[i] = new StringBuilder(args[i].ToString().Italic().Colored(argsColor));
        }

        var result = new StringBuilder();
        result.Append(channel.Bold());
        result.Append(": ");
        result.AppendFormat(cacheMessage, resultArgs);

        if (Thread.CurrentThread == mainThread)
        {
            InternalLog(result.ToString().Colored(defaultColor));
        }
        else
        {
            unityContext.Post(_ =>
            {
                InternalLog(result.ToString().Colored(defaultColor));
            }, null);
        }
    }

    /// <summary>
    /// Loguje informację w aplikacji.
    /// Informacje są zazwyczaj używane do logowania ogólnych informacji, które nie są błędami ani ostrzeżeniami.
    /// </summary>
    /// <param name="message">Wiadomość do zalogowania.</param>
    public static void LogInfo(object message)
    {
        if (!enabled) return;

        var cacheMessage = message == null ? "_____" : message.ToString();

        // Zapis do pliku
        if (Settings.All_SaveToFile && Settings.LogInfo_SaveToFile)
        {
            if (Settings.StackTrace_SaveToFile && Settings.StackTrace_LogInfo_SaveToFile)
            {
                // Zapis z StackTrace
                PrepareLogForSaving("", cacheMessage, LoggerType.Line, StackTraceUtility.ExtractStackTrace());
            }
            else
            {
                // Zapis bez StackTrace
                PrepareLogForSaving("", cacheMessage, LoggerType.Line);
            }
        }

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (!Settings.LogInfo_ShowInUnity) return;

        var result = new StringBuilder(cacheMessage.Colored(infoColor));

        if (Thread.CurrentThread == mainThread)
        {
            Debug.Log(result.ToString());
        }
        else
        {
            unityContext.Post(_ =>
            {
                Debug.Log(result.ToString());
            }, null);
        }
    }

    /// <summary>
    /// Loguje informację w aplikacji z określonym kanałem.
    /// Informacje są zazwyczaj używane do logowania ogólnych informacji, które nie są błędami ani ostrzeżeniami.
    /// </summary>
    /// <param name="channel">Kanał logowania.</param>
    /// <param name="message">Wiadomość do zalogowania.</param>
    public static void LogInfo(string channel, object message)
    {
        if (!enabled) return;

        var cacheMessage = message == null ? "_____" : message.ToString();

        // Zapis do pliku
        if (Settings.All_SaveToFile && Settings.Line_SaveToFile)
        {
            if (Settings.StackTrace_SaveToFile && Settings.StackTrace_LogInfo_SaveToFile)
            {
                // Zapis z StackTrace
                PrepareLogForSaving(channel, cacheMessage, LoggerType.Info, StackTraceUtility.ExtractStackTrace());
            }
            else
            {
                // Zapis bez StackTrace
                PrepareLogForSaving(channel, cacheMessage, LoggerType.Info);
            }
        }

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (!Settings.LogInfo_ShowInUnity) return;

        var result = new StringBuilder();
        result.Append(channel.Bold().Colored(infoColor));
        result.Append(": ");
        result.Append(cacheMessage.Colored(defaultColor));

        if (Thread.CurrentThread == mainThread)
        {
            Debug.Log(result.ToString());
        }
        else
        {
            unityContext.Post(_ =>
            {
                Debug.Log(result.ToString());
            }, null);
        }
    }

    /// <summary>
    /// Loguje informację w aplikacji z określonym kanałem.
    /// Informacje są zazwyczaj używane do logowania ogólnych informacji, które nie są błędami ani ostrzeżeniami.
    /// </summary>
    /// <param name="channel">Kanał logowania.</param>
    /// <param name="message">Wiadomość do zalogowania.</param>
    /// <param name="args">Argumenty do sformatowania wiadomości.</param>
    public static void LogInfo(string channel, object message, params object[] args)
    {
        if (!enabled) return;

        var cacheMessage = message == null ? "_____" : message.ToString();

        // Zapis do pliku
        if (Settings.All_SaveToFile && Settings.Line_SaveToFile)
        {
            if (Settings.StackTrace_SaveToFile && Settings.StackTrace_LogInfo_SaveToFile)
            {
                // Zapis z StackTrace
                PrepareLogForSaving(channel, string.Format(cacheMessage, args), LoggerType.Info, StackTraceUtility.ExtractStackTrace());
            }
            else
            {
                // Zapis bez StackTrace
                PrepareLogForSaving(channel, string.Format(cacheMessage, args), LoggerType.Info);
            }
        }

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (!Settings.LogInfo_ShowInUnity) return;

        // Formatowanie argumentów
        var resultArgs = new StringBuilder[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            resultArgs[i] = new StringBuilder(args[i].ToString().Italic().Colored(argsColor));
        }

        var result = new StringBuilder();
        result.Append(channel.Bold().Colored(infoColor));
        result.Append(": ");
        result.AppendFormat(cacheMessage.Colored(defaultColor), resultArgs);

        if (Thread.CurrentThread == mainThread)
        {
            Debug.Log(result.ToString());
        }
        else
        {
            unityContext.Post(_ =>
            {
                Debug.Log(result.ToString());
            }, null);
        }
    }

    /// <summary>
    /// Loguje ostrzeżenie w aplikacji.
    /// Ostrzeżenia są zazwyczaj używane do logowania sytuacji, które mogą być problematyczne, ale nie są krytyczne.
    /// </summary>
    /// <param name="message">Wiadomość do zalogowania.</param>
    public static void LogWarning(object message)
    {
        if (!enabled) return;

        var cacheMessage = message == null ? "_____" : message.ToString();

        // Zapis do pliku
        if (Settings.All_SaveToFile && Settings.LogWarning_SaveToFile)
        {
            if (Settings.StackTrace_SaveToFile && Settings.StackTrace_LogWarning_SaveToFile)
            {
                // Zapis z StackTrace
                PrepareLogForSaving("", cacheMessage, LoggerType.Line, StackTraceUtility.ExtractStackTrace());
            }
            else
            {
                // Zapis bez StackTrace
                PrepareLogForSaving("", cacheMessage, LoggerType.Line);
            }
        }

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (!Settings.LogWarning_ShowInUnity) return;

        var result = new StringBuilder(cacheMessage.Colored(warningColor));

        // Zmiana wyświetlania StackTrace dla unity
        var stackTraceState = Application.GetStackTraceLogType(LogType.Log);
        Application.SetStackTraceLogType(LogType.Log, Settings.StackTrace_ShowInUnity ? StackTraceLogType.ScriptOnly : StackTraceLogType.None);

        if (Thread.CurrentThread == mainThread)
        {
            Debug.LogWarning(result.ToString());
        }
        else
        {
            unityContext.Post(_ =>
            {
                Debug.LogWarning(result.ToString());
            }, null);
        }

        // Wyłączenie wyświetlania StackTrace dla unity
        Application.SetStackTraceLogType(LogType.Log, stackTraceState);
    }

    /// <summary>
    /// Loguje ostrzeżenie w aplikacji z określonym kanałem.
    /// Ostrzeżenia są zazwyczaj używane do logowania sytuacji, które mogą być problematyczne, ale nie są krytyczne.
    /// </summary>
    /// <param name="channel">Kanał, z którego pochodzi ostrzeżenie.</param>
    /// <param name="message">Wiadomość do zalogowania.</param>
    public static void LogWarning(string channel, object message)
    {
        if (!enabled) return;

        var cacheMessage = message == null ? "_____" : message.ToString();

        // Zapis do pliku
        if (Settings.All_SaveToFile && Settings.LogWarning_SaveToFile)
        {
            if (Settings.StackTrace_SaveToFile && Settings.StackTrace_LogWarning_SaveToFile)
            {
                // Zapis z StackTrace
                PrepareLogForSaving(channel, cacheMessage, LoggerType.Warning, StackTraceUtility.ExtractStackTrace());
            }
            else
            {
                // Zapis bez StackTrace
                PrepareLogForSaving(channel, cacheMessage, LoggerType.Warning);
            }
        }

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (!Settings.LogWarning_ShowInUnity) return;

        var result = new StringBuilder();
        result.Append(channel.Bold().Colored(warningColor));
        result.Append(": ");
        result.Append(cacheMessage.Colored(defaultColor));

        if (Thread.CurrentThread == mainThread)
        {
            Debug.LogWarning(result.ToString());
        }
        else
        {
            unityContext.Post(_ =>
            {
                Debug.LogWarning(result.ToString());
            }, null);
        }
    }

    /// <summary>
    /// Loguje ostrzeżenie w aplikacji z określonym kanałem.
    /// Ostrzeżenia są zazwyczaj używane do logowania sytuacji, które mogą być problematyczne, ale nie są krytyczne.
    /// </summary>
    /// <param name="channel">Kanał, z którego pochodzi ostrzeżenie.</param>
    /// <param name="message">Wiadomość do zalogowania.</param>
    /// <param name="args">Argumenty do sformatowania wiadomości.</param>
    public static void LogWarning(string channel, object message, params object[] args)
    {
        if (!enabled) return;

        var cacheMessage = message == null ? "_____" : message.ToString();

        // Zapis do pliku
        if (Settings.All_SaveToFile && Settings.LogWarning_SaveToFile)
        {
            if (Settings.StackTrace_SaveToFile && Settings.StackTrace_LogWarning_SaveToFile)
            {
                // Zapis z StackTrace
                PrepareLogForSaving(channel, string.Format(cacheMessage, args), LoggerType.Warning, StackTraceUtility.ExtractStackTrace());
            }
            else
            {
                // Zapis bez StackTrace
                PrepareLogForSaving(channel, string.Format(cacheMessage, args), LoggerType.Warning);
            }
        }

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (!Settings.LogWarning_ShowInUnity) return;

        // Formatowanie argumentów
        var resultArgs = new StringBuilder[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            resultArgs[i] = new StringBuilder(args[i].ToString().Italic().Colored(argsColor));
        }

        var result = new StringBuilder();
        result.Append(channel.Bold().Colored(warningColor));
        result.Append(": ");
        result.AppendFormat(cacheMessage.Colored(defaultColor), resultArgs);

        if (Thread.CurrentThread == mainThread)
        {
            Debug.LogWarning(result.ToString());
        }
        else
        {
            unityContext.Post(_ =>
            {
                Debug.LogWarning(result.ToString());
            }, null);
        }
    }

    /// <summary>
    /// Loguje błąd w aplikacji.
    /// Błędy są zazwyczaj używane do logowania sytuacji, które są krytyczne i wymagają natychmiastowej uwagi.
    /// </summary>
    /// <param name="message">Wiadomość do zalogowania.</param>
    public static void LogError(object message)
    {
        if (!enabled) return;

        var cacheMessage = message == null ? "_____" : message.ToString();

        // Zapis do pliku
        if (Settings.All_SaveToFile && Settings.LogError_SaveToFile)
        {
            if (Settings.StackTrace_SaveToFile && Settings.StackTrace_LogError_SaveToFile)
            {
                // Zapis z StackTrace
                PrepareLogForSaving("", cacheMessage, LoggerType.Line, StackTraceUtility.ExtractStackTrace());
            }
            else
            {
                // Zapis bez StackTrace
                PrepareLogForSaving("", cacheMessage, LoggerType.Line);
            }
        }

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (!Settings.LogError_ShowInUnity) return;

        var result = new StringBuilder(cacheMessage.Colored(errorColor));

        if (Thread.CurrentThread == mainThread)
        {
            Debug.LogError(result.ToString());
        }
        else
        {
            unityContext.Post(_ =>
            {
                Debug.LogError(result.ToString());
            }, null);
        }
    }

    /// <summary>
    /// Loguje błąd w aplikacji.
    /// </summary>
    /// <param name="channel">Kanał logowania.</param>
    /// <param name="message">Wiadomość do zalogowania.</param>
    public static void LogError(string channel, object message)
    {
        if (!enabled) return;

        var cacheMessage = message == null ? "_____" : message.ToString();

        // Zapis do pliku
        if (Settings.All_SaveToFile && Settings.LogError_SaveToFile)
        {
            if (Settings.StackTrace_SaveToFile && Settings.StackTrace_LogError_SaveToFile)
            {
                // Zapis z StackTrace
                PrepareLogForSaving(channel, cacheMessage, LoggerType.Error, StackTraceUtility.ExtractStackTrace());
            }
            else
            {
                // Zapis bez StackTrace
                PrepareLogForSaving(channel, cacheMessage, LoggerType.Error);
            }
        }

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (!Settings.LogError_ShowInUnity) return;

        var result = new StringBuilder();
        result.Append(channel.Bold().Colored(errorColor));
        result.Append(": ");
        result.Append(cacheMessage.Colored(defaultColor));

        if (Thread.CurrentThread == mainThread)
        {
            Debug.LogError(result.ToString());
        }
        else
        {
            unityContext.Post(_ =>
            {
                Debug.LogError(result.ToString());
            }, null);
        }
    }

    /// <summary>
    /// Loguje błąd w aplikacji.
    /// </summary>
    /// <param name="channel">Kanał logowania.</param>
    /// <param name="message">Wiadomość do zalogowania.</param>
    /// <param name="args">Argumenty do sformatowania wiadomości.</param>
    public static void LogError(string channel, object message, params object[] args)
    {
        if (!enabled) return;

        var cacheMessage = message == null ? "_____" : message.ToString();

        // Zapis do pliku
        if (Settings.All_SaveToFile)
        {
            if (Settings.StackTrace_SaveToFile)
            {
                // Zapis z StackTrace
                PrepareLogForSaving(channel, string.Format(cacheMessage, args), LoggerType.Error, StackTraceUtility.ExtractStackTrace());
            }
            else
            {
                // Zapis bez StackTrace
                PrepareLogForSaving(channel, string.Format(cacheMessage, args), LoggerType.Error);
            }
        }

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (!Settings.LogError_ShowInUnity) return;

        // Formatowanie argumentów
        var resultArgs = new StringBuilder[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            resultArgs[i] = new StringBuilder(args[i].ToString().Italic().Colored(argsColor));
        }

        var result = new StringBuilder();
        result.Append(channel.Bold().Colored(errorColor));
        result.Append(": ");
        result.AppendFormat(cacheMessage.Colored(defaultColor), resultArgs);

        if (Thread.CurrentThread == mainThread)
        {
            Debug.LogError(result.ToString());
        }
        else
        {
            unityContext.Post(_ =>
            {
                Debug.LogError(result.ToString());
            }, null);
        }
    }

    /// <summary>
    /// Zapisuje wyjątek do konsoli Unity, oraz zapisuje do pliku.
    /// Jeśli aplikacja nie jest w trybie debugowania, to zapis do konsoli jest pomijany.
    /// </summary>
    /// <param name="exception">Wyjątek do zapisania.</param>
    public static void LogException(Exception exception)
    {
        if (!enabled) return;

        // Zapis do konsoli
        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;

        if (Thread.CurrentThread == mainThread)
        {
            Debug.LogException(exception);
        }
        else
        {
            unityContext.Post(_ =>
            {
                Debug.LogException(exception);
            }, null);
        }
    }

    /// <summary>
    /// Wyświetla separator w konsoli Unity, oraz zapisuje do pliku.
    /// </summary>
    public static void Separator()
    {
        if (!enabled) return;

        if (Settings.All_SaveToFile)
        {
            PrepareLogForSaving(string.Empty, string.Empty, LoggerType.Separator);
        }

        if (!Settings.IsDebugBuild) return;
        if (!Settings.All_ShowInUnity) return;
        if (Thread.CurrentThread == mainThread)
        {
            Debug.Log(separator);
        }
        else
        {
            unityContext.Post(_ =>
            {
                Debug.Log(separator);
            }, null);
        }
    }

    /// <summary>
    /// Wyświetla StackTrace w konsoli Unity, oraz zapisuje do pliku.
    /// </summary>
    public static void StackTrace()
    {
        if (!enabled) return;

        if (!Settings.IsDebugBuild) return;

        string stackTrace = StackTraceUtility.ExtractStackTrace();

        if (Settings.All_SaveToFile && Settings.StackTrace_SaveToFile)
        {
            PrepareLogForSaving(string.Empty, string.Empty, LoggerType.StackTrace, stackTrace);
        }

        if (!Settings.All_ShowInUnity) return;
        if (!Settings.StackTrace_ShowInUnity) return;

        if (Thread.CurrentThread == mainThread)
        {
            Debug.Log(FormatStackTraceForUnity(stackTrace));
        }
        else
        {
            unityContext.Post(_ =>
            {
                Debug.Log(FormatStackTraceForUnity(stackTrace));
            }, null);
        }
    }
}