# Настройка эффектов камеры для UI элементов

## 🎥 Проблема: Эффекты камеры не работают с UI

### Причина:
UI Canvas по умолчанию рендерится в **Screen Space - Overlay**, что означает, что он не проходит через камеру и эффекты не применяются.

## ✅ Решение: Настройка Canvas для работы с камерой

### Шаг 1: Настройка Canvas
1. **Выберите Canvas** в иерархии
2. **В Inspector найдите Canvas компонент**
3. **Измените Render Mode** на **Screen Space - Camera**
4. **Назначьте Main Camera** в поле Render Camera
5. **Установите Plane Distance = 1**

### Шаг 2: Настройка камеры
1. **Выберите Main Camera**
2. **В Camera компоненте:**
   - Projection = Orthographic
   - Size = 5 (или подходящий размер)
   - Near = 0.1
   - Far = 10

### Шаг 3: Добавление эффектов
1. **Добавьте скрипт `UICameraEffects`** к Main Camera
2. **Настройте ссылки:**
   - UI Camera: Main Camera
   - Canvas: ваш Canvas
3. **Настройте параметры:**
   - Fish Eye Strength: 0.3
   - Chromatic Aberration: 0.1
   - Vignette: 0.3

## 🔧 Альтернативный способ: Эффекты для отдельных элементов

### Для кнопок с fish eye эффектом:
1. **Добавьте скрипт `UIFishEyeEffect`** к каждой кнопке
2. **Настройте ссылки:**
   - Target Transform: RectTransform кнопки
   - Target Image: Image компонент кнопки
3. **Настройте параметры:**
   - Fish Eye Strength: 0.3
   - Animate Effect: ✓ (для анимации)

### Для автоматической настройки Canvas:
1. **Добавьте скрипт `CanvasCameraSetup`** к Canvas
2. **Настройте ссылки:**
   - Canvas: ваш Canvas
   - UI Camera: Main Camera
3. **Скрипт автоматически настроит Canvas**

## 📋 Пошаговая настройка:

### 1. **Основная настройка:**
```
Main Camera
├── UICameraEffects скрипт
└── RetroCamera скрипт (если есть)

Canvas
├── Render Mode: Screen Space - Camera
├── Render Camera: Main Camera
├── Plane Distance: 1
└── CanvasCameraSetup скрипт
```

### 2. **Для отдельных кнопок:**
```
Button
├── UIFishEyeEffect скрипт
├── Target Transform: RectTransform
└── Target Image: Image
```

## 🎮 Ожидаемый результат:

### **С настройкой Canvas:**
- Все UI элементы проходят через камеру
- Применяются все эффекты (fish eye, хроматическая аберрация, виньетка)
- Кнопки деформируются вместе с фоном

### **С отдельными эффектами:**
- Каждая кнопка имеет свой fish eye эффект
- Эффект зависит от позиции относительно центра экрана
- Можно настроить анимацию эффекта

## 🐛 Если не работает:

### **Проверьте настройки Canvas:**
1. Render Mode должен быть **Screen Space - Camera**
2. Render Camera должен быть назначен
3. Plane Distance должен быть > 0

### **Проверьте настройки камеры:**
1. Projection должен быть **Orthographic**
2. Size должен быть подходящим (обычно 5)
3. Near и Far должны быть настроены правильно

### **Проверьте Console:**
- "CanvasCameraSetup: Canvas настроен для работы с камерой"
- "CanvasCameraSetup: Камера настроена для UI"

## ✅ Преимущества:

- **Все UI элементы** проходят через камеру
- **Эффекты применяются** ко всему интерфейсу
- **Кнопки деформируются** вместе с фоном
- **Ретро атмосфера** сохраняется

Теперь ваши кнопки будут деформироваться вместе с камерой! 🎯
