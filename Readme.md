# Debug Logger (DL)

**Debug Logger (DL)** to uniwersalna biblioteka do logowania i debugowania aplikacji, zaprojektowana z myślą o środowisku Unity, ale możliwa do użycia także w innych projektach .NET. Umożliwia wygodne logowanie informacji, ostrzeżeń, błędów, wyjątków oraz śladów stosu zarówno do konsoli Unity, jak i do plików.

## Najważniejsze funkcje

- Logowanie różnych typów wiadomości: informacyjne, ostrzeżenia, błędy, wyjątki, linie, separatory.
- Obsługa logowania do pliku z buforowaniem i automatycznym zarządzaniem liczbą plików.
- Kolorowanie i formatowanie logów w konsoli Unity.
- Możliwość wyłączenia/włączenia logowania jednym przełącznikiem.
- Konfigurowalne ustawienia przez `DLSettings`.
- Automatyczne przechwytywanie wyjątków z Unity.

## Instalacja

1. Skopiuj pliki z katalogu `DL` do swojego projektu Unity lub projektu .NET.
2. Upewnij się, że masz referencję do `UnityEngine.dll` (jeśli używasz w Unity).

## Szybki start

```csharp
// Inicjalizacja DL z domyślnymi ustawieniami
DL.Initialize();

// Proste logowanie
DL.Log("To jest zwykły log.");

// Logowanie informacji
DL.LogInfo("To jest informacja.");

// Logowanie ostrzeżenia
DL.LogWarning("To jest ostrzeżenie!");

// Logowanie błędu
DL.LogError("To jest błąd!");

// Logowanie wyjątku
try
{
    throw new System.Exception("Przykładowy wyjątek");
}
catch (System.Exception ex)
{
    DL.LogException(ex);
}

// Logowanie z kanałem
DL.Log("SYSTEM", "Log z kanałem");

// Logowanie z formatowaniem
DL.Log("SYSTEM", "Wartość: {0}, status: {1}", 123, "OK");

// Separator w logach
DL.Separator();

// Wyświetlenie śladu stosu
DL.StackTrace();
```

## Konfiguracja

Możesz skonfigurować zachowanie loggera przez przekazanie własnych ustawień:

```csharp
var settings = new DL.DLSettings
{
    Buffer_Enabled = true,
    Buffer_Size = 20,
    Buffer_Timeout = 10,
    FileLogMaxCount = 5,
    IsDebugBuild = true,
    All_ShowInUnity = true,
    All_SaveToFile = true,
    // ... inne ustawienia ...
};

DL.Initialize(settings);
```

## Wyłączenie/włączenie logowania

```csharp
DL.Enabled = false; // Wyłącza wszystkie logi
DL.Enabled = true;  // Włącza logowanie ponownie
```

## Czyszczenie logów i wymuszenie zapisu

```csharp
DL.ClearLogList();    // Czyści bufor logów
DL.ForceSaveToFile(); // Wymusza natychmiastowy zapis do pliku
```

## Informacje o wersji

```csharp
string info = DL.Information();
Debug.Log(info);
```

## Wskazówki

- Domyślnie logi są zapisywane do katalogu `Application.persistentDataPath` w Unity.
- Liczba plików logów jest ograniczona przez `FileLogMaxCount` (domyślnie 10).
- Kolorowanie i formatowanie działa tylko w konsoli Unity.

## Licencja

Projekt przeznaczony do użytku wewnętrznego. Jeśli chcesz użyć go w swoim projekcie, skontaktuj się z autorem.

---

**Autor:** Ebbi Gebbi
**Wersja:** v1.4 (2025-06-13)
