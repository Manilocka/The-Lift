# Диагностика UI проблем

## 🔍 Как узнать в чем была проблема

### 1. Автоматическая диагностика
1. **Добавьте скрипт `UIDiagnostics`** к любому GameObject
2. **В Unity Editor** нажмите кнопку **"Diagnose All UI"** в Inspector
3. **Проверьте Console** - там будет подробная информация о всех UI элементах

### 2. Что покажет диагностика:
- **Количество Canvas** в сцене
- **Настройки каждого Canvas** (Render Mode, Camera, Plane Distance)
- **Количество RectTransform** элементов
- **Элементы с экстремальными позициями** (с подробной информацией)
- **Компоненты** каждого проблемного элемента

### 3. Пример вывода в Console:
```
=== UI DIAGNOSTICS START ===
Найдено Canvas: 2
Canvas: MainMenuCanvas
  - Render Mode: ScreenSpaceOverlay
  - World Camera: None
  - Plane Distance: 0
  - Sorting Order: 0
Найдено RectTransform: 15
ЭКСТРЕМАЛЬНАЯ ПОЗИЦИЯ: SettingsButton
  - Позиция: (0, 0, 16739.585938)
  - Родитель: MenuPanel
  - Размер: (200, 50)
  - Якоря: min=(0.5,0.5), max=(0.5,0.5)
  - Компоненты: RectTransform, Button, Image, Text
Элементов с экстремальными позициями: 1
=== UI DIAGNOSTICS END ===
```

### 4. Постоянный мониторинг
- **Включите `logOnUpdate = true`** для постоянного мониторинга
- **Каждую секунду** скрипт будет проверять экстремальные позиции
- **В Console** будут появляться предупреждения о новых проблемах

### 5. Автоматическое исправление
- **Нажмите "Fix All Extreme Positions"** для автоматического исправления
- **Все экстремальные позиции** будут сброшены в (0,0,0)
- **В Console** появится сообщение о количестве исправленных элементов

## 🎯 Типичные причины проблем:

### 1. **Неправильные настройки Canvas**
- Render Mode = Screen Space - Camera без назначенной камеры
- Неправильный Plane Distance
- Отсутствие Canvas Scaler

### 2. **Экстремальные позиции UI элементов**
- Z-координата > 10000 (часто из-за анимаций)
- X/Y координаты за пределами экрана
- Неправильные якоря (Anchors)

### 3. **Проблемы с родительскими объектами**
- UI элементы в неправильной иерархии
- Отсутствие Canvas как родителя
- Неправильные Transform компоненты

## 🔧 Решение проблем:

### Для Canvas:
```csharp
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
// ИЛИ
canvas.renderMode = RenderMode.ScreenSpaceCamera;
canvas.worldCamera = Camera.main;
canvas.planeDistance = 1f;
```

### Для UI элементов:
```csharp
rectTransform.anchoredPosition = Vector3.zero;
rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
```

## 📋 Чек-лист диагностики:

- [ ] Все Canvas имеют правильный Render Mode
- [ ] Нет элементов с Z > 1000
- [ ] Все UI элементы имеют правильных родителей
- [ ] Canvas Scaler настроен правильно
- [ ] Нет дублирующихся Canvas
- [ ] Все кнопки имеют правильные позиции
