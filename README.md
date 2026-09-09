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