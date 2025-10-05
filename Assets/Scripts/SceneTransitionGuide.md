# Руководство по настройке переходов между сценами

## Настройка переходов в главном меню

### 1. Настройка сцен в Build Settings

1. Откройте **File > Build Settings**
2. Добавьте все сцены в Build Settings:
   - `Game menu` (индекс 0)
   - `day1_e` (индекс 1) 
   - `day1_m` (индекс 2)
   - Другие игровые сцены

### 2. Настройка RetroMainMenu

В скрипте `RetroMainMenu.cs` уже настроены переменные:
```csharp
[Header("Scene Management")]
public string gameSceneName = "day1_e"; // Имя сцены игры
public string settingsSceneName = "Settings"; // Имя сцены настроек
```

### 3. Как работает "Продолжить"

1. **Проверка сохранения**: Скрипт проверяет `PlayerPrefs.GetInt("GameProgress", 0)`
2. **Если есть сохранение**: Кнопка активна, загружается последний уровень
3. **Если нет сохранения**: Кнопка неактивна (серого цвета)

### 4. Как работает "Новая игра"

1. Очищает все сохранения (`PlayerPrefs.DeleteKey("GameProgress")`)
2. Загружает начальную сцену игры
3. Создает новое сохранение

### 5. Система сохранений

Используйте `SaveManager.cs` для управления сохранениями:

```csharp
// Сохранить игру
SaveManager.Instance.SaveGame(levelNumber);

// Проверить наличие сохранения
bool hasSave = SaveManager.Instance.HasSaveData();

// Удалить сохранение
SaveManager.Instance.DeleteSave();
```

### 6. Настройка переходов в игре

В игровых сценах добавьте:

```csharp
// Возврат в главное меню
public void ReturnToMainMenu()
{
    SceneManager.LoadScene("Game menu");
}

// Сохранение прогресса
public void SaveProgress()
{
    SaveManager.Instance.SaveGame(currentLevel);
}
```

### 7. Настройка кнопки "Продолжить"

В Unity Inspector для `RetroMainMenu`:
1. **Game Scene Name**: `day1_e` (или нужная сцена)
2. **Settings Scene Name**: `Settings` (если есть сцена настроек)
3. **Selection Background**: Image компонент 700x70 с цветом #43FCBD

### 8. Тестирование

1. Запустите игру
2. Проверьте, что кнопка "Продолжить" неактивна при первом запуске
3. Создайте сохранение в игре
4. Вернитесь в меню - кнопка должна стать активной
5. Проверьте переходы между сценами

### 9. Дополнительные настройки

#### Создание сцены настроек (опционально):
1. Создайте новую сцену `Settings`
2. Добавьте UI элементы для настроек
3. Добавьте кнопку "Назад в меню"
4. Добавьте сцену в Build Settings

#### Создание системы уровней:
```csharp
// В игровой сцене
public class LevelManager : MonoBehaviour
{
    public int currentLevel = 1;
    
    void Start()
    {
        // Загружаем сохраненный уровень
        if (SaveManager.Instance.HasSaveData())
        {
            currentLevel = SaveManager.Instance.GetCurrentLevel();
        }
    }
    
    public void CompleteLevel()
    {
        currentLevel++;
        SaveManager.Instance.SaveGame(currentLevel);
    }
}
```

### 10. Отладка

Если переходы не работают:
1. Проверьте имена сцен в Build Settings
2. Убедитесь, что сцены добавлены в Build Settings
3. Проверьте Console на ошибки
4. Убедитесь, что SaveManager не уничтожается между сценами
