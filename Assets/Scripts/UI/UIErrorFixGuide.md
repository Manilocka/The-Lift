# Исправление ошибки "Screen position out of view frustum"

## 🐛 Проблема
Ошибка `Screen position out of view frustum` возникает когда UI элементы находятся за пределами видимой области камеры.

## ✅ Решение

### 1. Автоматическое исправление
1. **Добавьте скрипт `UIPositionFixer`** к Canvas в сцене
2. **Добавьте скрипт `CameraBoundsFixer`** к Main Camera
3. **Добавьте скрипт `EmergencyUIFixer`** к любому GameObject для экстренного исправления
4. **Запустите сцену** - скрипты автоматически исправят позиции

### 1.1. Экстренное исправление
Если ошибка продолжается:
1. **В Unity Editor** выберите GameObject с `EmergencyUIFixer`
2. **В Inspector** нажмите кнопку **"Emergency Fix All UI"**
3. **Или в коде** вызовите `EmergencyUIFixer.ForceFixUI()`

### 2. Ручное исправление в Unity Editor

#### Для Canvas:
1. **Выберите Canvas** в иерархии
2. **В Inspector найдите Canvas компонент**
3. **Установите Render Mode = Screen Space - Overlay**
4. **Если используется Screen Space - Camera:**
   - Назначьте Main Camera в Render Camera
   - Установите Plane Distance = 1

#### Для UI элементов:
1. **Выберите проблемный UI элемент**
2. **В RectTransform компоненте:**
   - Anchored Position: (0, 0) или разумные значения (-1000 до 1000)
   - Size Delta: разумные размеры (не более 2000x2000)
   - Anchors: установите в центр (0.5, 0.5) или растяните (0,0) до (1,1)

#### Для Camera:
1. **Выберите Main Camera**
2. **В Camera компоненте:**
   - Projection = Orthographic
   - Size = 5 (или подходящий размер)
   - Near = 0.1
   - Far = 10
   - Rect: X=0, Y=0, W=1, H=1

### 3. Проверка и профилактика

#### Проверьте настройки Canvas:
```csharp
// В скрипте проверьте:
Canvas canvas = GetComponent<Canvas>();
if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
{
    canvas.worldCamera = Camera.main;
    canvas.planeDistance = 1f;
}
```

#### Проверьте позиции UI элементов:
```csharp
// Проверьте позиции всех RectTransform
RectTransform rect = GetComponent<RectTransform>();
Vector3 pos = rect.anchoredPosition;
if (Mathf.Abs(pos.x) > 2000f || Mathf.Abs(pos.y) > 2000f)
{
    rect.anchoredPosition = Vector3.zero; // Сброс позиции
}
```

## 🔧 Дополнительные настройки

### Для меню паузы:
1. **Убедитесь что PauseMenu Canvas** имеет правильные настройки
2. **Проверьте что слайдеры настроек** находятся в видимой области
3. **Установите правильные Anchors** для адаптивности

### Для главного меню:
1. **Проверьте RetroMainMenu Canvas** настройки
2. **Убедитесь что кнопки меню** находятся в центре экрана
3. **Проверьте Video Background** не перекрывает UI

## 🎯 Ожидаемый результат
- Ошибка "Screen position out of view frustum" исчезнет
- UI элементы будут правильно отображаться
- Камера будет иметь правильные границы
- Все элементы интерфейса будут в видимой области

## 🚨 Если проблема остается:

### Экстренные меры:
1. **Добавьте `EmergencyUIFixer`** к любому GameObject
2. **В Inspector** нажмите **"Emergency Fix All UI"**
3. **Проверьте Console** - должны появиться сообщения об исправлении

### Дополнительная проверка:
1. **Проверьте Console** на другие ошибки
2. **Убедитесь что все UI элементы** имеют правильные родительские объекты
3. **Проверьте что Canvas Scaler** настроен правильно
4. **Убедитесь что разрешение экрана** соответствует настройкам UI

### Код для принудительного исправления:
```csharp
// В любом скрипте добавьте:
EmergencyUIFixer fixer = FindObjectOfType<EmergencyUIFixer>();
if (fixer != null)
{
    fixer.ForceFixUI();
}
```
