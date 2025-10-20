# DialogueColliderInteraction - Инструкция по настройке

## Описание
Скрипт `DialogueColliderInteraction` позволяет показывать диалог при попадании игрока в коллайдер или при выходе из него.

## Настройка в Unity

### 1. Настройка объекта с коллайдером:
- Добавьте компонент `DialogueColliderInteraction` на объект
- Убедитесь, что у объекта есть `Collider` с включенным `Is Trigger`
- Убедитесь, что у игрока есть тег `Player`

### 2. Настройка диалога:
- В поле `Dialogue` перетащите созданный DialogueSO (например, `ColliderInteractionDialogue`)
- Настройте параметры:
  - `One Time Only` - показывать диалог только один раз
  - `Trigger On Enter` - срабатывать при входе в коллайдер
  - `Trigger On Exit` - срабатывать при выходе из коллайдера
  - `Delay Before Dialogue` - задержка перед показом диалога (секунды)

### 3. Создание нового диалога:
1. В Project окне щелкните правой кнопкой мыши
2. Выберите `Create > Dialogue`
3. Настройте строки диалога в Inspector
4. Перетащите созданный диалог в поле `Dialogue` компонента

## Примеры использования

### Пример 1: Диалог при входе в зону
- `Trigger On Enter` = true
- `Trigger On Exit` = false
- `One Time Only` = true

### Пример 2: Диалог при выходе из зоны
- `Trigger On Enter` = false
- `Trigger On Exit` = true
- `One Time Only` = false

### Пример 3: Диалог с задержкой
- `Delay Before Dialogue` = 2.0f
- Диалог появится через 2 секунды после входа в зону

## Публичные методы

```csharp
// Установить новый диалог
dialogueInteraction.SetDialogue(newDialogue);

// Сбросить активацию (позволить показать диалог снова)
dialogueInteraction.ResetActivation();

// Проверить, находится ли игрок внутри
bool isInside = dialogueInteraction.IsPlayerInside();

// Проверить, был ли диалог уже показан
bool wasActivated = dialogueInteraction.HasBeenActivated();

// Принудительно показать диалог
dialogueInteraction.ForceShowDialogue();
```

## Пример использования
Создан пример диалога `ColliderInteractionDialogue.asset` в папке `Assets/Dialogue/` для тестирования функциональности.

## Отладка
Включите `Log Debug` для получения подробных сообщений в консоли Unity.
