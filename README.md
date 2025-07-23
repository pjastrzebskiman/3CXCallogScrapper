# 3CXCallogScrapper

## Opis aplikacji

Aplikacja **3CXCallogScrapper** służy do pobierania i archiwizowania logów połączeń z systemu 3CX. Komunikuje się z API 3CX, pobiera dane o połączeniach w zadanych interwałach czasowych i zapisuje je do bazy danych PostgreSQL. Może być uruchamiana jako usługa lub aplikacja konsolowa.

## Plik konfiguracyjny `appsettings.json`

Plik `appsettings.json` zawiera wszystkie kluczowe ustawienia aplikacji. Poniżej opis poszczególnych sekcji i parametrów:

### 1. Logging
- **LogLevel**: Określa poziom szczegółowości logów dla aplikacji oraz bibliotek Microsoft. Przykładowe wartości: `Information`, `Warning`, `Error`.

### 2. ConnectionStrings
- **PostgreSQL**: Łańcuch połączenia do bazy danych PostgreSQL, gdzie zapisywane są logi połączeń. Zawiera adres hosta, nazwę bazy, użytkownika i hasło.

### 3. 3CXApiSettings
- **BaseUrl**: Adres bazowy API systemu 3CX, z którym łączy się aplikacja.
- **AuthEndpoint**: Endpoint API służący do uwierzytelniania i pobierania tokenu dostępowego.
- **CallLogEndpoint**: Endpoint API do pobierania danych o połączeniach (logów).
- **Username**: Nazwa użytkownika do logowania w API 3CX.
- **Password**: Hasło użytkownika do logowania w API 3CX.
- **SecurityCode**: Kod bezpieczeństwa, jeśli wymagany przez API (może być pusty).
- **QueryIntervalMinutes**: Co ile minut aplikacja pobiera nowe logi z API 3CX.
- **LookbackMinutes**: Liczba minut wstecz, z jakiego okresu mają być pobierane logi przy każdym zapytaniu.
- **GetOldCalls**: Flaga logiczna (true/false), czy przy starcie aplikacji pobierać starsze połączenia (historyczne).
- **StartTimeGetOldCalls**: Data i czas (w formacie ISO), od kiedy mają być pobierane historyczne połączenia, jeśli `GetOldCalls` jest ustawione na true.

## Przykład pliku `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Database=3cxcalllog;Username=postgres;Password=..."
  },
  "3CXApiSettings": {
    "BaseUrl": "https://adres.3cx.pl",
    "AuthEndpoint": "/webclient/api/Login/GetAccessToken",
    "CallLogEndpoint": "/xapi/v1/ReportCallLogData/Pbx.GetCallLogData",
    "Username": "100",
    "Password": "...",
    "SecurityCode": "",
    "QueryIntervalMinutes": 15,
    "LookbackMinutes": 30,
    "GetOldCalls": true,
    "StartTimeGetOldCalls": "2025-03-01T00:00:00Z"
  }
}
```

## Uruchomienie aplikacji

1. Skonfiguruj plik `appsettings.json` zgodnie z powyższym opisem.
2. Upewnij się, że baza danych PostgreSQL jest dostępna i skonfigurowana.
3. Uruchom aplikację (np. `dotnet run` lub jako usługę).

## Kontakt
W razie pytań lub problemów skontaktuj się z autorem aplikacji.