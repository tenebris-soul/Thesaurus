# Thesaurus

[Русский](#thesaurus-ru) | [English](#thesaurus-en)

<a id="thesaurus-ru"></a>

# Thesaurus — Русский

Thesaurus — ассет-плагин для Unity 6, предназначенный для создания интерактивных виртуальных музеев. Плагин помогает быстро собрать музейное пространство, добавить экспонаты, артефакты и картины, настроить точки интереса, UI-описания и базовое взаимодействие игрока с объектами сцены.

Проект подходит для классических виртуальных музеев, цифровых галерей, образовательных экспозиций и прототипов интерактивных музейных сценариев.

## Возможности

- Готовый контроллер игрока для перемещения по музейной сцене.
- Система взаимодействия с экспонатами через коллайдеры и камеру игрока.
- Поддержка обычных экспонатов с описанием и точкой просмотра.
- Поддержка артефактов с несколькими точками интереса.
- Поддержка картин с отдельными интерактивными участками.
- UI на Unity UI Toolkit для отображения информации об объектах.
- ScriptableObject-данные для экспонатов, артефактов, картин и конфигурации игрока.
- Префабы `[System]` и `Player` для быстрого подключения плагина к сцене.
- Готовые тестовые сцены и примеры данных.

## Требования

- Unity 6.
- Zenject.
- CySharp/UniTask.
- Unity Input System.
- Базовые навыки работы с Unity-сценами, префабами, коллайдерами и ScriptableObject.

## Структура репозитория

```text
Thesaurus/
  Configs/          # ScriptableObject-конфиги игрока и звуков
  InputActions/     # Input System actions
  Materials/        # Материалы и тестовые текстуры
  Prefabs/          # Готовые префабы [System] и Player
  Scenes/           # Тестовые сцены и пример музейной экспозиции
  Scripts/          # Код плагина
  Settings/         # Render Pipeline и графические настройки
  Sounds/           # Звуки шагов и скольжения
  UI Toolkit/       # UXML/USS интерфейсы и UI-ассеты
```

Основные группы скриптов:

- `Scripts/Exhibits` — обычные экспонаты: `ExhibitData`, `ExhibitObject`, `ExhibitsRegistry`.
- `Scripts/Artifacts` — артефакты: `ArtifactData`, `ArtifactObject`, `ArtifactRegistry`.
- `Scripts/Paintings` — картины и точки интереса: `PaintingData`, `PaintingObject`, `PaintingInterestPointData`, `PaintingInterestPointObject`.
- `Scripts/Player` — контроллер игрока, камера, ввод, режимы взаимодействия, UI и сервисы инспекции объектов.
- `Scripts/Surface` — регистрация поверхностей для звуков и взаимодействия с окружением.

## Установка

1. Создайте или откройте проект в Unity 6.
2. Установите зависимости `Zenject` и `CySharp/UniTask`.
3. Убедитесь, что в проекте включён Unity Input System.
4. Добавьте папку `Thesaurus` в `Assets` вашего Unity-проекта.
5. Откройте нужную сцену или создайте новую музейную сцену.
6. Перетащите на сцену префабы:
   - `Thesaurus/Prefabs/[System].prefab`
   - `Thesaurus/Prefabs/Player.prefab`
7. Выделите `[System]` и в компоненте `Scene Context` добавьте `Player` в поле `Mono Installers`.

После этого можно размещать музейные объекты и подключать к ним соответствующие компоненты.

## Быстрый старт

1. Подготовьте пространство музея: комнату, зал, импортированный уровень или набор 3D-объектов.
2. Разместите в сцене будущие музейные объекты.
3. Добавьте на объекты коллайдеры. Для большинства случаев подойдёт `BoxCollider`.
4. Создайте данные объекта через меню `Create`.
5. Назначьте данные и вспомогательные объекты в инспекторе.
6. Запустите сцену и проверьте взаимодействие через контроллер игрока.

## Обычный экспонат

Обычный экспонат использует `ExhibitData` и компонент `ExhibitObject`. Такой объект имеет название, описание и отдельную позицию камеры для просмотра.

### Настройка

1. Создайте папку `ExhibitsData`.
2. Создайте данные: `Create -> Exhibits -> ExhibitData`.
3. Заполните название и описание экспоната.
4. На сцене создайте два пустых объекта:
   - `CameraAnchor`
   - `CameraTarget`
5. Разместите `CameraAnchor` и `CameraTarget` так, чтобы экспонат корректно попадал в кадр при просмотре.
6. Добавьте на экспонат коллайдер.
7. Добавьте компонент `ExhibitObject`.
8. Назначьте в `ExhibitObject`:
   - `ExhibitData`
   - `CameraTarget`
   - `CameraAnchor`
9. Объедините экспонат и вспомогательные объекты в общего родителя.

## Артефакт

Артефакт использует `ArtifactData` и компонент `ArtifactObject`. В отличие от обычного экспоната, артефакт может иметь несколько точек интереса. Каждая точка раскрывает отдельный факт или деталь объекта.

### Настройка

1. Создайте папку `ArtifactsData`.
2. Создайте данные: `Create -> Artifacts -> ArtifactData`.
3. Заполните название, описание и факты об артефакте.
4. Создайте на сцене точки интереса по количеству фактов.
5. Добавьте на каждую точку интереса коллайдер.
6. Добавьте на артефакт компонент `ArtifactObject`.
7. Назначьте в `ArtifactObject`:
   - `ArtifactData`
   - массив `Interest Points`
8. Объедините артефакт и точки интереса в общего родителя.

## Картина

Картина использует `PaintingData`, `PaintingObject` и отдельные `PaintingInterestPointData` для интерактивных участков изображения.

### Настройка

1. Создайте папку `PaintingsData`.
2. Создайте данные картины: `Create -> Painting -> PaintingData`.
3. Заполните название картины.
4. Создайте один или несколько файлов `PaintingInterestPointData`.
5. В каждом `PaintingInterestPointData` укажите описание отдельного участка картины.
6. Создайте на сцене точки интереса и добавьте на них коллайдеры.
7. Добавьте на картину компонент `PaintingObject`.
8. Назначьте в `PaintingObject` созданный `PaintingData`.
9. Настройте направление камеры через параметр `Axis`.
10. Объедините картину и точки интереса в общего родителя.

## Проверка сцены

Перед запуском убедитесь, что:

- На сцене есть префабы `[System]` и `Player`.
- В `Scene Context` у `[System]` назначен `Player` в поле `Mono Installers`.
- У всех интерактивных объектов и точек интереса есть коллайдеры.
- В `ExhibitObject`, `ArtifactObject` и `PaintingObject` заполнены все обязательные поля.
- Файлы данных названы понятно и соответствуют объектам сцены.
- `CameraAnchor` и `CameraTarget` у обычных экспонатов расположены корректно.
- Точки интереса артефактов и картин находятся на нужных участках объектов.

## Частые ошибки

| Проблема | Что проверить |
| --- | --- |
| Объект не реагирует на взаимодействие | Проверьте, есть ли на объекте или точке интереса коллайдер, и назначен ли нужный компонент. |
| В инспекторе пустые поля | Проверьте, назначены ли ScriptableObject-данные и ссылки на объекты сцены. |
| Камера смотрит не туда | Проверьте расположение `CameraAnchor` и `CameraTarget`. Для картин дополнительно настройте `Axis`. |
| Зависимости не работают | Убедитесь, что `Zenject` и `CySharp/UniTask` установлены до запуска сцены. |
| UI не отображается | Проверьте наличие файлов из `UI Toolkit` и корректность префаба `Player`. |

## Рекомендации по организации проекта

Храните данные разных типов объектов отдельно:

```text
ExhibitsData/
ArtifactsData/
PaintingsData/
```

Для сценовых объектов используйте понятные имена, например:

```text
Vase_01
Painting_MonaLisa
Artifact_AncientMask
```

Вспомогательные объекты `CameraAnchor`, `CameraTarget` и точки интереса лучше держать внутри родительского объекта конкретного экспоната. Так сцена остаётся аккуратной, а связанные элементы не теряются при переносе или копировании.

## Лицензия

Проект распространяется по лицензии MIT. См. файл [LICENSE](LICENSE).
---

<a id="thesaurus-en"></a>

# Thesaurus — English
Thesaurus is a Unity 6 asset plugin for building interactive virtual museums. It helps you quickly assemble museum spaces, add exhibits, artifacts, and paintings, configure points of interest, UI descriptions, and basic player interaction with museum objects.

The project is suitable for classic virtual museums, digital galleries, educational exhibitions, and prototypes of interactive museum experiences.

## Features

- Ready-to-use player controller for moving through a museum scene.
- Object interaction system based on colliders and the player camera.
- Support for regular exhibits with descriptions and dedicated viewing positions.
- Support for artifacts with multiple points of interest.
- Support for paintings with separate interactive image areas.
- Unity UI Toolkit interfaces for displaying object information.
- ScriptableObject data for exhibits, artifacts, paintings, and player configuration.
- `[System]` and `Player` prefabs for quick scene setup.
- Test scenes and example data included.

## Requirements

- Unity 6.
- Zenject.
- CySharp/UniTask.
- Unity Input System.
- Basic knowledge of Unity scenes, prefabs, colliders, and ScriptableObject assets.

## Repository Structure

```text
Thesaurus/
  Configs/          # ScriptableObject configs for player behavior and sounds
  InputActions/     # Input System actions
  Materials/        # Materials and test textures
  Prefabs/          # Ready-to-use [System] and Player prefabs
  Scenes/           # Test scenes and example museum exhibition
  Scripts/          # Plugin source code
  Settings/         # Render Pipeline and graphics settings
  Sounds/           # Footstep and sliding sounds
  UI Toolkit/       # UXML/USS interfaces and UI assets
```

Main script groups:

- `Scripts/Exhibits` — regular exhibits: `ExhibitData`, `ExhibitObject`, `ExhibitsRegistry`.
- `Scripts/Artifacts` — artifacts: `ArtifactData`, `ArtifactObject`, `ArtifactRegistry`.
- `Scripts/Paintings` — paintings and points of interest: `PaintingData`, `PaintingObject`, `PaintingInterestPointData`, `PaintingInterestPointObject`.
- `Scripts/Player` — player controller, camera, input, interaction modes, UI, and object inspection services.
- `Scripts/Surface` — surface registration for sounds and environment interaction.

## Installation

1. Create or open a Unity 6 project.
2. Install the `Zenject` and `CySharp/UniTask` dependencies.
3. Make sure Unity Input System is enabled in the project.
4. Add the `Thesaurus` folder to your Unity project's `Assets` folder.
5. Open an existing scene or create a new museum scene.
6. Drag the following prefabs into the scene:
   - `Thesaurus/Prefabs/[System].prefab`
   - `Thesaurus/Prefabs/Player.prefab`
7. Select `[System]` and, in the `Scene Context` component, add `Player` to the `Mono Installers` field.

After that, you can place museum objects in the scene and attach the corresponding components to them.

## Quick Start

1. Prepare the museum space: a room, hall, imported level, or a set of 3D objects.
2. Place future museum objects in the scene.
3. Add colliders to the objects. `BoxCollider` is suitable for most cases.
4. Create object data through the `Create` menu.
5. Assign the data and helper scene objects in the Inspector.
6. Run the scene and test interaction through the player controller.

## Regular Exhibit

A regular exhibit uses `ExhibitData` and the `ExhibitObject` component. This object has a title, description, and a dedicated camera position for viewing.

### Setup

1. Create an `ExhibitsData` folder.
2. Create data: `Create -> Exhibits -> ExhibitData`.
3. Fill in the exhibit title and description.
4. Create two empty objects in the scene:
   - `CameraAnchor`
   - `CameraTarget`
5. Position `CameraAnchor` and `CameraTarget` so the exhibit is framed correctly during viewing.
6. Add a collider to the exhibit.
7. Add the `ExhibitObject` component.
8. Assign the following fields in `ExhibitObject`:
   - `ExhibitData`
   - `CameraTarget`
   - `CameraAnchor`
9. Group the exhibit and helper objects under a common parent.

## Artifact

An artifact uses `ArtifactData` and the `ArtifactObject` component. Unlike a regular exhibit, an artifact can contain multiple points of interest. Each point reveals an additional fact or detail about the object.

### Setup

1. Create an `ArtifactsData` folder.
2. Create data: `Create -> Artifacts -> ArtifactData`.
3. Fill in the artifact title, description, and facts.
4. Create points of interest in the scene according to the number of facts.
5. Add a collider to each point of interest.
6. Add the `ArtifactObject` component to the artifact.
7. Assign the following fields in `ArtifactObject`:
   - `ArtifactData`
   - `Interest Points` array
8. Group the artifact and its points of interest under a common parent.

## Painting

A painting uses `PaintingData`, `PaintingObject`, and separate `PaintingInterestPointData` assets for interactive image areas.

### Setup

1. Create a `PaintingsData` folder.
2. Create painting data: `Create -> Painting -> PaintingData`.
3. Fill in the painting title.
4. Create one or more `PaintingInterestPointData` files.
5. In each `PaintingInterestPointData`, describe a separate area of the painting.
6. Create points of interest in the scene and add colliders to them.
7. Add the `PaintingObject` component to the painting.
8. Assign the created `PaintingData` in `PaintingObject`.
9. Configure the camera direction using the `Axis` parameter.
10. Group the painting and its points of interest under a common parent.

## Scene Checklist

Before running the scene, make sure that:

- The `[System]` and `Player` prefabs are present in the scene.
- `Player` is assigned to the `Mono Installers` field in `[System]`'s `Scene Context`.
- All interactive objects and points of interest have colliders.
- All required fields in `ExhibitObject`, `ArtifactObject`, and `PaintingObject` are filled in.
- Data files are clearly named and match the scene objects.
- `CameraAnchor` and `CameraTarget` are positioned correctly for regular exhibits.
- Artifact and painting points of interest are placed on the correct object areas.

## Common Issues

| Issue | What to check |
| --- | --- |
| Object does not react to interaction | Check whether the object or point of interest has a collider and the correct component assigned. |
| Empty fields in the Inspector | Check whether ScriptableObject data and scene object references are assigned. |
| Camera points in the wrong direction | Check `CameraAnchor` and `CameraTarget`. For paintings, also configure `Axis`. |
| Dependencies do not work | Make sure `Zenject` and `CySharp/UniTask` are installed before running the scene. |
| UI is not displayed | Check that `UI Toolkit` files are present and the `Player` prefab is configured correctly. |

## Project Organization Tips

Keep data for different object types in separate folders:

```text
ExhibitsData/
ArtifactsData/
PaintingsData/
```

Use clear names for scene objects, for example:

```text
Vase_01
Painting_MonaLisa
Artifact_AncientMask
```

Keep helper objects such as `CameraAnchor`, `CameraTarget`, and points of interest inside the parent object of the corresponding exhibit. This keeps the scene organized and prevents related objects from being lost when moving or duplicating assets.

## License

This project is distributed under the MIT License. See [LICENSE](LICENSE).


