# Пользовательский интерфейс

Пользовательский интерфейс (UI) в CollectorBots построен на основе Unity UI (uGUI) с использованием TextMesh Pro для текста. Архитектура UI следует паттерну MVP (Model‑View‑Presenter), где Presenter выступает посредником между игровыми данными и визуальными элементами.

## Структура Canvas и иерархия UI

В сцене присутствует один главный Canvas, который содержит все UI‑элементы. Иерархия выглядит следующим образом:

```
Canvas (Render Mode: Screen Space - Overlay)
├── EventSystem
├── ResourceManagementView (префаб)
├── StorageView (префаб)
├── BaseStructurePanelGroup (префаб)
└── Input (префаб)
```

Каждый из этих префабов инстанцируется при инициализации (через `Bootstrap` или соответствующий презентер).

## ResourceManagementView — управление задачами

`ResourceManagementView` отображает текущие задачи рабочих в трёх категориях:

- **Готовые задачи** (Ready) — ресурсы, которые можно собрать (ожидают назначения рабочего).
- **Назначенные задачи** (Assigned) — задачи, которые уже назначены на конкретного рабочего.
- **Завершённые задачи** (Completed) — задачи, которые были успешно выполнены (ресурс доставлен на базу).

### Визуальное представление

Каждая категория представлена вертикальной панелью с заголовком и списком задач. Задача отображается как иконка ресурса + текст (например, “Stone x1”).

### Презентер

`ResourceTaskManagementPresenter` подписывается на события:
- `TaskAssigned` — добавляет задачу в список «Назначенные».
- `TaskCompleted` — перемещает задачу в «Завершённые».
- `ResourceSpawned` — добавляет задачу в «Готовые».

При изменении списка презентер обновляет View, вызывая методы `AddReadyTask`, `MoveToAssigned` и т.д.

## StorageView — отображение ресурсов

`StorageView` показывает текущее количество ресурсов в хранилище выбранной базы.

### Элементы интерфейса

Для каждого типа ресурса (камень, дерево, железо) есть отдельная строка, содержащая:

- **Иконка ресурса** (спрайт из `Assets/Resources/Textures/Icons/`).
- **Название** (текст на английском: “Stone”, “Wood”, “Iron”).
- **Количество** (число, обновляемое в реальном времени).

### Презентер

`ResourceStoragePresenter` подписывается на событие `ResourceStorageChangedEvent`. Когда количество ресурсов меняется, презентер получает новое значение и вызывает `StorageView.SetResourceCount(resourceType, count)`.

## BaseStructurePanelGroup — управление базой

Эта панель появляется, когда игрок выбирает базу (кликает на неё). Она содержит:

- **Кнопка “Build Worker”** — запускает постройку нового рабочего (если хватает ресурсов).
- **Кнопка “Place Flag”** — позволяет разместить флаг для указания зоны сбора.
- **Информация о базе** — название, количество рабочих, уровень (не реализовано).

### Презентер

`BaseStructurePresenter` создаётся фабрикой `BaseStructurePresenterFactory` при выборе базы. Он подписывается на события базы (например, `WorkerBuilt`) и обновляет UI. При нажатии кнопок презентер вызывает соответствующие методы на `BaseStructure` (например, `StartBuildingWorker`).

## Связь Presenter → View → EventAggregator

Архитектура UI построена по принципу реактивного программирования: изменения в игровом состоянии публикуются как события, а презентеры реагируют на них, обновляя View.

### Пример потока данных:

1. Рабочий доставляет ресурс на базу.
2. База публикует `ResourceCollectedEvent`.
3. `ResourceStoragePresenter` (подписанный на это событие) получает событие.
4. Презентер запрашивает у модели (`ResourceStorage`) новое количество ресурсов.
5. Презентер вызывает `StorageView.SetResourceCount(...)`.
6. View обновляет текст на экране.

### Код примера:

```csharp
public class ResourceStoragePresenter : IPresenter
{
    private readonly StorageView _view;
    private readonly IEventAggregator _eventAggregator;
    private readonly ResourceStorage _storage;

    public ResourceStoragePresenter(StorageView view, IEventAggregator eventAggregator, ResourceStorage storage)
    {
        _view = view;
        _eventAggregator = eventAggregator;
        _storage = storage;

        _eventAggregator.Subscribe<ResourceStorageChangedEvent>(OnStorageChanged);
    }

    private void OnStorageChanged(ResourceStorageChangedEvent evt)
    {
        var count = _storage.GetCount(evt.ResourceType);
        _view.SetResourceCount(evt.ResourceType, count);
    }
}
```

## Шрифты

Во всём UI используется шрифт **CrayonLibre‑Regular** (из пакета TextMesh Pro). Он загружается как `TMP_FontAsset` из `Assets/Resources/Fonts/CrayonLibre‑Regular SDF.asset`.

### Настройка шрифта:

- **Размер по умолчанию**: 24px.
- **Стиль**: Regular (обычный), без начертаний.
- **Цвет**: белый (#FFFFFF) с чёрной тенью для улучшения читаемости.

## Адаптивность

UI разработан для разрешения 1920×1080, но использует **Anchor Presets** для корректного отображения на разных экранах:

- `StorageView` закреплён в правом верхнем углу.
- `ResourceManagementView` — в левом верхнем углу.
- `BaseStructurePanelGroup` — в нижней части экрана по центру.

При изменении размера окна элементы сохраняют свои относительные позиции.

## Анимации UI

Простые анимации (появление/исчезновение панелей, подсветка кнопок) реализованы через **DOTween**. Например, при выборе базы панель `BaseStructurePanelGroup` плавно выезжает снизу.

```csharp
_view.PanelTransform.DOLocalMoveY(targetY, 0.3f).SetEase(Ease.OutBack);
```

## Отладка UI

Для отладки UI в редакторе можно использовать:

- **Game View** с разными разрешениями (через меню “Game” → “Aspect Ratio”).
- **UI Debugger** (Window → Analysis → UI Debugger) для анализа иерархии и свойств элементов.
- **Console** — логи от презентеров (например, “Presenter initialized”, “View updated”).

## Рекомендации по расширению

1. **Добавление нового UI‑элемента**:
   - Создайте префаб с необходимыми компонентами (CanvasRenderer, Image, TextMeshPro‑Text и т.д.).
   - Напишите View‑скрипт, реализующий интерфейс `IView` (если используется) или просто публичные методы для обновления.
   - Создайте Presenter, который будет подписываться на соответствующие события и обновлять View.
   - Зарегистрируйте Presenter в `Bootstrap` или фабрике.

2. **Локализация**:
   - Замените жёстко закодированные строки на ключи (например, “STORAGE_STONE”).
   - Создайте систему локализации, которая будет загружать словарь из JSON и подставлять значения в `TextMeshPro` компоненты.

3. **Оптимизация**:
   - Избегайте частых обновлений UI каждый кадр. Используйте события для реактивных обновлений.
   - Объединяйте несколько изменений в один кадр (например, через `LayoutRebuilder.ForceRebuildLayoutImmediate` только один раз).