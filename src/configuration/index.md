# Конфигурация и настройки

В проекте CollectorBots для хранения настроек используются **ScriptableObject** ассеты. Это позволяет изменять баланс игры без перекомпиляции кода, а также удобно настраивать различные параметры в редакторе Unity.

## ScriptableObjects

Все конфигурационные файлы находятся в папке `Assets/Resources/Configs/`. Они загружаются через `StaticDataProvider` и доступны всем системам.

### BaseStructureConfig

Настройки базовой структуры (главной и вторичной).

**Поля:**

| Поле | Тип | Описание |
|------|-----|----------|
| `WaitingArea` | `Area` | Зона, где рабочие ожидают задачи. |
| `DeliveryArea` | `Area` | Зона, куда рабочие доставляют ресурсы. |
| `SpawnArea` | `Area` | Зона, где появляются новые рабочие после постройки. |
| `ScanRadius` | `float` | Радиус сканирования ресурсов (в мировых единицах). |
| `ScanInterval` | `float` | Интервал между сканированиями (в секундах). |
| `InitialWorkerCount` | `int` | Начальное количество рабочих у базы. |
| `TaskAssignmentInterval` | `float` | Интервал между назначениями задач (в секундах). |

**Примеры значений:**

- **StartBaseStructureConfig** (главная база):
  - `ScanRadius`: 278.4
  - `ScanInterval`: 1
  - `InitialWorkerCount`: 3
  - `TaskAssignmentInterval`: 1

- **SecondaryBaseStructureConfig** (вторичная база):
  - `ScanRadius`: 278.4
  - `ScanInterval`: 1
  - `InitialWorkerCount`: 0
  - `TaskAssignmentInterval`: 1

### BuilderConfig

Настройки строительства (баз и рабочих).

**Поля:**

| Поле | Тип | Описание |
|------|-----|----------|
| `BaseCost` | `ResourceSpendRequest[]` | Массив запросов ресурсов для постройки базы. |
| `WorkerCost` | `ResourceSpendRequest[]` | Массив запросов ресурсов для постройки рабочего. |
| `BaseBuildTime` | `float` | Время постройки базы (в секундах). |
| `WorkerBuildTime` | `float` | Время постройки рабочего (в секундах). |

**Пример значений (DefaultBuilderConfig):**

- **BaseCost**:
  - Камень (Stone) ×2
  - Дерево (Wood) ×1
  - Железо (Iron) ×2
- **WorkerCost**:
  - Камень (Stone) ×1
  - Дерево (Wood) ×2
  - Железо (Iron) ×0
- **BaseBuildTime**: 20 секунд
- **WorkerBuildTime**: 10 секунд

### ResourceSpawnerConfig

Настройки спавна ресурсов.

**Поля:**

| Поле | Тип | Описание |
|------|-----|----------|
| `ResourceWeights` | `ResourceWeight[]` | Массив весов для каждого типа ресурса. |
| `SpawnArea` | `Rectangle` | Прямоугольная область, в которой спавнятся ресурсы. |
| `SpawnInterval` | `float` | Интервал между спавнами (в секундах). |
| `MaxResources` | `int` | Максимальное количество ресурсов одновременно на карте. |

**Пример значений (DefaultResourceSpawnerConfig):**

- **ResourceWeights**:
  - Stone: вес 100
  - Wood: вес 95
  - Iron: вес 90
- **SpawnArea**: прямоугольник от (-72.5, 105) до (72.5, -112.5)
- **SpawnInterval**: 4 секунды
- **MaxResources**: 50 (предположительно)

### ResourceWeight

Вспомогательная структура, используемая в `ResourceSpawnerConfig`.

**Поля:**

| Поле | Тип | Описание |
|------|-----|----------|
| `ResourceType` | `ResourceType` | Тип ресурса (Stone, Wood, Iron). |
| `Weight` | `int` | Вес для взвешенного случайного выбора (чем больше, тем чаще появляется). |

## Настройки навигации (NavMesh)

Навигационная сеть построена для поверхности Ground. Параметры NavMesh заданы в сцене `SampleScene` через окно **Navigation** (Window → AI → Navigation).

### Настройки NavMeshAgent у рабочего

Компонент `NavMeshAgent` на префабе `Worker.prefab` имеет следующие параметры:

- **Speed**: 3.5
- **Angular Speed**: 120
- **Acceleration**: 8
- **Stopping Distance**: 0.5
- **Auto Braking**: true
- **Radius**: 0.5
- **Height**: 2
- **Base Offset**: 0

Эти значения обеспечивают плавное и реалистичное движение робота.

### Область навигации

NavMesh покрывает всю поверхность Ground, за исключением участков под базами и ресурсами (они отмечены как Not Walkable). Это предотвращает попытки рабочих пройти сквозь объекты.

## Настройки DOTween

DOTween используется для простых анимаций (появление/исчезновение UI, плавное перемещение камеры). Глобальные настройки хранятся в ассете `Assets/Resources/DOTweenSettings.asset`.

### Параметры по умолчанию:

- **Default Ease**: `Ease.OutQuad`
- **Default AutoPlay**: `AutoPlay.All`
- **Default UpdateType**: `UpdateType.Normal`
- **Default TimeScaleIndependent**: false
- **Default UseSafeMode**: true
- **Default LogBehaviour**: `LogBehaviour.Default`

### Использование в коде:

```csharp
// Плавное перемещение объекта за 1 секунду
transform.DOMove(targetPosition, 1f);

// Плавное изменение альфа‑канала UI изображения
image.DOFade(0, 0.5f);
```

## Настройки камеры

Камера в сцене имеет следующие компоненты:

- **Transform**: позиция (0, 50, -50), поворот (45, 0, 0).
- **Camera**: Projection = Orthographic, Size = 30.
- **Скрипт CameraController**: обрабатывает перемещение и зум через `InputReader`.

Параметры управления камерой (скорость перемещения, границы) заданы прямо в скрипте `CameraController` (не вынесены в конфиг).

## Настройки физики

Проект использует стандартные настройки физики Unity (Edit → Project Settings → Physics):

- **Gravity**: (0, -9.81, 0)
- **Default Material**: None
- **Bounce Threshold**: 2
- **Default Contact Offset**: 0.01

Коллайдеры используются только для Ground (Terrain Collider) и ресурсов (Box Collider). Рабочие и базы не используют физику для движения (только NavMesh).

## Настройки освещения

Освещение в сцене `SampleScene` настроено как **Baked Global Illumination**:

- **Directional Light** (солнце) с интенсивностью 1.0 и цветом белый.
- **Ambient Source**: Skybox.
- **Environment Lighting**: Intensity Multiplier = 1.0.

Такие настройки обеспечивают статичное, но приятное освещение без динамических теней (для производительности).

## Настройки качества (Quality Settings)

Проект использует стандартные настройки качества Unity для платформы PC (Edit → Project Settings → Quality):

- **Pixel Light Count**: 4
- **Texture Quality**: Full Res
- **Anisotropic Textures**: Per Texture
- **Anti‑Aliasing**: 2x Multi Sampling
- **Soft Particles**: On
- **Shadows**: Hard and Soft Shadows, Shadow Distance 150
- **Shadow Cascades**: 4 Cascades
- **Shadow Resolution**: High

## Как изменить конфигурацию

1. Откройте папку `Assets/Resources/Configs/` в окне Project.
2. Выберите нужный ассет (например, `DefaultBuilderConfig`).
3. Измените значения в Inspector.
4. Сохраните сцену (изменения применяются немедленно, но для работы в собранной игре нужно пересобрать проект).

Для добавления новой конфигурации создайте ScriptableObject, унаследованный от нужного базового класса, и поместите его в ту же папку. Затем загрузите через `StaticDataProvider.GetConfig<T>()`.