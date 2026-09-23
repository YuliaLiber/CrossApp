# CrossApp

Наскрізний проект з крос-платформного програмування.

Предметна область: Замовлення.

Сутності:
- Customer — клієнт
- Product — товар
- Order — замовлення
- OrderLine — рядок замовлення

Призначення: оформлення замовлень і підрахунок сум.

## Запуск

```bash
dotnet build
dotnet run --project src/Cli
```

## Середовище

.NET SDK 10.0  
macOS 14.6  
RID: osx-arm64

## Self-contained публікація

Було створено self-contained збірки для двох RID:

- osx-arm64 — 83M
- linux-arm64 — 85M

Збірка для linux-arm64 має трохи більший розмір.

## Лабораторна робота 2

У solution тепер є два проєкти:

- `Core` — бібліотека класів, яка збирає інформацію про середовище;
- `Cli` — консольний застосунок, який використовує `Core` і форматує вивід.

Залежність між проєктами:

`Cli → Core`

### Структура

```text
CrossApp/
├── CrossApp.sln
├── README.md
├── .gitignore
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   └── EnvironmentInfo.cs
    └── Cli/
        ├── Cli.csproj
        └── Program.cs

```

### Основні команди

Збірка:

```bash
dotnet build
```

Запуск:

```bash
dotnet run --project src/Cli
```

Self-contained публікація:

```bash
dotnet publish src/Cli -c Release -r osx-arm64 --self-contained true
```

Framework-dependent публікація:

```bash
dotnet publish src/Cli -c Release -r osx-arm64 --self-contained false
```

### Порівняння публікацій

| RID | Режим | Розмір | Потрібен встановлений Runtime |
|---|---|---:|---|
| osx-arm64 | self-contained | 83M | Ні |
| osx-arm64 | framework-dependent | 172K | Так (.NET 10) |
| osx-arm64 | single-file | 76M | Ні |
| osx-arm64 | trimmed | 20M | Ні |

У режимі `PublishSingleFile` кількість файлів зменшилась з 193 до 3.

При використанні `PublishTrimmed` розмір зменшився до 20M. Під час збірки з'явилися 2 попередження `IL2026`, пов'язані з JSON-серіалізацією та trimming.

### Multi-targeting

Бібліотека `Core` збирається для двох TFM:

- `net8.0`
- `net10.0`

Для перевірки умовної компіляції використовується `BuildNote`:

- при збірці під `net8.0` виводиться `збірка під net8.0`;
- при збірці під `net10.0` виводиться `збірка під net10.0`.

## Лабораторна робота 3

У лабораторній роботі реалізовано імпорт даних для домену «Замовлення».

### Формат CSV

Файл використовує роздільник `;`.

Формат:

```text
id;name;price
```

Приклад:

```text
F-001;Тумба приліжкова Nord;1890.00
```

Ціна зчитується через `decimal.TryParse` з `CultureInfo.InvariantCulture`.

Пошкоджені рядки не зупиняють імпорт, а додаються до списку помилок разом із номером рядка.

### Підтримувані формати

* `.csv` — `ProductCsvImporter`
* `.json` — `ProductJsonImporter`

Вибір імпортера виконується за розширенням файлу через `switch expression`.

### Додаткові завдання

Реалізовано:

* імпорт даних із JSON;
* вибір між CSV та JSON;
* розпізнавання різнорідних рядків за префіксом `P` / `W`;
* статистику імпорту: загальна кількість записів, кількість прийнятих і пропущених записів та відсоток помилок.

### Приклади запуску

```bash
dotnet run --project src/Cli
dotnet run --project src/Cli -- data/sample-valid.csv
dotnet run --project src/Cli -- data/sample.json
dotnet run --project src/Cli -- --mixed data/mixed-sample.txt
dotnet run --project src/Cli -- data/no-file.csv
```
