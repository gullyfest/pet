# aeris OS — Unity 2D

Интерфейс операционной системы в стиле **aeris Aero** (середина 2000-х: глянец, скруглённые рамки, прозрачные стёкла, насыщенные градиенты).

## Запуск

> Полный гайд: [`../HOW_TO_RUN.md`](../HOW_TO_RUN.md)

1. Открыть проект в **Unity 6000.4.4f1** через Unity Hub (выбрать папку `Pet/`).
2. Открыть `Assets/Scenes/SampleScene.unity`.
3. Нажать **Play**.

Ничего вручную собирать в редакторе не нужно — `GameBootstrap` через
`[RuntimeInitializeOnLoadMethod]` сам создаёт Canvas, EventSystem,
AudioManager и весь UI на старте сцены. Текстуры и звуки генерируются
процедурно, внешних ассетов нет.

## Что внутри

- Главное меню (Start / Settings / Exit) с глянцевой «капсулой».
- Экран загрузки с прогрессбаром и fade-переходами.
- Рабочий стол с 5 иконками (My Computer, Notes, Music, Browser, Settings).
- Перетаскиваемые окна с кнопками закрытия / минимизации.
- Системная панель: Start-орб, виджет погоды, часы (`DateTime.Now`), tray.
- Toast-уведомления в правом верхнем углу.
- Синтезированные beep-звуки (click / success / notify) — без `.wav` файлов.

## Структура

```
Assets/Scripts/
├── Utils/         ColorPalette, TextureFactory
├── Managers/      AudioManager, SceneFlowManager, GameBootstrap
└── UI/            aerisButton, DraggableWindow, WindowManager,
                   DesktopIcon, DesktopBuilder, SystemPanel,
                   MainMenuUI, LoadingScreen,
                   NotificationToast, NotificationSystem
```

Все скрипты в неймспейсах `aerisOS.Utils / .Managers / .UI`. Комментарии — на английском.
