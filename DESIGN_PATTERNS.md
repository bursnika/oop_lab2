# Патерни проектування в проекті

Детальний опис реалізації патернів проектування у лабораторній роботі.

## 1. Strategy (Стратегія)

### Призначення
Визначає сімейство алгоритмів, інкапсулює кожен з них і робить їх взаємозамінними.

### Реалізація у проекті

**Контекст**: Парсинг XML файлів може бути виконаний різними способами.

#### Інтерфейс стратегії
```csharp
public interface IXmlParser
{
    List<Book> ParseBooks(string xmlFilePath, SearchCriteria? criteria = null);
    List<Reader> ParseReaders(string xmlFilePath, SearchCriteria? criteria = null);
    List<BorrowedBook> ParseBorrowedBooks(string xmlFilePath, SearchCriteria? criteria = null);
    string GetParserName();
}
```

#### Конкретні стратегії

**1. SaxXmlParser** - Event-driven парсинг
- Використовує XmlReader
- Мінімальне використання пам'яті
- Підходить для великих файлів
- Швидкий, але більш складний код

**2. DomXmlParser** - DOM-based парсинг
- Використовує XmlDocument
- Завантажує весь XML в пам'ять
- Простий доступ до будь-якого елемента
- Більше використання пам'яті

**3. LinqXmlParser** - LINQ-based парсинг
- Використовує XDocument
- Сучасний підхід з LINQ запитами
- Читабельний та елегантний код
- Баланс між швидкістю та зручністю

#### Контекст стратегії
```csharp
public class XmlParserContext
{
    private IXmlParser _parser;

    public void SetParser(IXmlParser parser)
    {
        _parser = parser;
    }

    public List<Book> ParseBooks(string xmlFilePath, SearchCriteria? criteria = null)
    {
        return _parser.ParseBooks(xmlFilePath, criteria);
    }
}
```

#### Використання
```csharp
var context = new XmlParserContext(new LinqXmlParser());
context.SetParser(new SaxXmlParser()); // Зміна стратегії в runtime
var books = context.ParseBooks("library.xml");
```

### Переваги реалізації
- Легко додати новий спосіб парсингу
- Можна міняти парсер в runtime (через UI)
- Кожен парсер ізольований і тестується окремо
- Відповідає принципу Open/Closed

---

## 2. Factory Method (Фабричний метод)

### Призначення
Визначає інтерфейс для створення об'єкта, але залишає підкласам рішення про те, який клас інстанціювати.

### Реалізація у проекті

**Контекст**: Експорт даних може бути в різних форматах (XML, HTML).

#### Базовий клас з фабричним методом
```csharp
public abstract class FileExporterBase
{
    protected string LoadedXmlContent { get; private set; }

    public void LoadXmlFile(string xmlFilePath)
    {
        LoadedXmlContent = File.ReadAllText(xmlFilePath);
    }

    // Фабричний метод
    public abstract IFileExporter CreateExporter();

    public async Task<string> ExportAsync(string fileName)
    {
        var exporter = CreateExporter(); // Виклик фабричного методу
        return await exporter.ExportAsync(LoadedXmlContent, fileName);
    }
}
```

#### Конкретні фабрики

**1. XmlExporterFactory**
```csharp
public class XmlExporterFactory : FileExporterBase
{
    public override IFileExporter CreateExporter()
    {
        return new XmlFileExporter();
    }
}
```

**2. HtmlExporterFactory**
```csharp
public class HtmlExporterFactory : FileExporterBase
{
    private readonly string _xsltFilePath;

    public override IFileExporter CreateExporter()
    {
        return new HtmlFileExporter(_xsltFilePath);
    }
}
```

#### Продукти фабрики
```csharp
public interface IFileExporter
{
    Task<string> ExportAsync(string content, string fileName);
    string GetFileExtension();
    string GetExporterName();
}
```

#### Використання
```csharp
// XML експорт
var xmlFactory = new XmlExporterFactory();
xmlFactory.LoadXmlFile("library.xml");
await xmlFactory.ExportAsync("output");

// HTML експорт
var htmlFactory = new HtmlExporterFactory("library.xslt");
htmlFactory.LoadXmlFile("library.xml");
await htmlFactory.ExportAsync("output");
```

### Переваги реалізації
- Легко додати нові формати експорту
- Інкапсуляція логіки створення експортерів
- Спільний код (LoadXmlFile) в базовому класі
- Відповідає Single Responsibility Principle

---

## 3. Singleton (Одинак)

### Призначення
Гарантує що клас має тільки один екземпляр і надає глобальну точку доступу до нього.

### Реалізація у проекті

**Контекст**: Система логування повинна мати один екземпляр для всього застосунку.

#### Реалізація
```csharp
public sealed class EventLogger
{
    private static EventLogger? _instance;
    private static readonly object _lock = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    // Приватний конструктор
    private EventLogger()
    {
        var logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        if (!Directory.Exists(logsDirectory))
        {
            Directory.CreateDirectory(logsDirectory);
        }

        _logFilePath = Path.Combine(logsDirectory, $"library_log_{DateTime.Now:yyyyMMdd}.txt");
    }

    // Публічна точка доступу
    public static EventLogger Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock) // Double-checked locking
                {
                    if (_instance == null)
                    {
                        _instance = new EventLogger();
                    }
                }
            }
            return _instance;
        }
    }

    // Потокобезпечне логування
    public async Task LogAsync(LogLevel level, string message)
    {
        await _semaphore.WaitAsync();
        try
        {
            var timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            var levelName = GetLevelName(level);
            var logEntry = $"{timestamp} {levelName}. {message}";

            await File.AppendAllTextAsync(_logFilePath, logEntry + Environment.NewLine);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

#### Рівні логування
```csharp
public enum LogLevel
{
    Filtering,      // Фільтрація даних
    Transformation, // Трансформація в HTML
    Saving         // Збереження файлів
}
```

#### Використання
```csharp
var logger = EventLogger.Instance;
logger.Log(LogLevel.Filtering, "Параметри: Title=Clean Code");
logger.Log(LogLevel.Transformation, "Збережено у файл output.html");
logger.Log(LogLevel.Saving, "Збережено відфільтрований фрагмент");
```

### Переваги реалізації
- **Thread-safe**: Використовує double-checked locking
- **Lazy initialization**: Створюється тільки при першому використанні
- **Sealed class**: Неможливо успадкувати
- **SemaphoreSlim**: Забезпечує потокобезпечний доступ до файлу
- Єдина точка логування для всього застосунку

---

## Комбінування патернів

У проекті патерни працюють разом:

```csharp
// Strategy для парсингу
var parser = new XmlParserContext(new LinqXmlParser());
var books = parser.ParseBooks("library.xml");

// Factory Method для експорту
var factory = new HtmlExporterFactory("library.xslt");
factory.LoadXmlFile("library.xml");
var outputPath = await factory.ExportAsync("result");

// Singleton для логування
EventLogger.Instance.Log(LogLevel.Saving, $"Файл збережено: {outputPath}");
```

---

## Anti-patterns уникнені

### Magic Pushbutton (Магічна кнопка)
❌ **Неправильно**: Одна кнопка робить все
```csharp
// ПОГАНО
private void MagicButton_Click()
{
    LoadFile();
    ParseXml();
    FilterData();
    TransformToHtml();
    SaveFile();
    UploadToCloud();
}
```

✅ **Правильно**: Окремі кнопки для окремих дій
- Вибрати файл
- Пошук
- Трансформація
- Експорт XML
- Експорт HTML
- Google Drive
- Clear

### God Object (Божественний об'єкт)
❌ **Неправильно**: Один клас робить все

✅ **Правильно**: Розділення відповідальностей
- `XmlParser` - тільки парсинг
- `FileExporter` - тільки експорт
- `EventLogger` - тільки логування
- `GoogleDriveService` - тільки робота з Google Drive
- `XsltTransformationService` - тільки трансформація

---

## Принципи SOLID

### Single Responsibility Principle
Кожен клас має одну відповідальність:
- `SaxXmlParser` - парсинг через SAX
- `XmlFileExporter` - експорт в XML
- `EventLogger` - логування

### Open/Closed Principle
Відкрито для розширення, закрито для модифікації:
- Додати новий парсер: створити клас з `IXmlParser`
- Додати новий експортер: створити клас з `IFileExporter`

### Liskov Substitution Principle
Всі парсери взаємозамінні через `IXmlParser`

### Interface Segregation Principle
Інтерфейси специфічні та невеликі:
- `IXmlParser` - тільки парсинг
- `IFileExporter` - тільки експорт

### Dependency Inversion Principle
Залежності від абстракцій, не від конкретних класів:
```csharp
private IXmlParser _parser; // Не SaxXmlParser
```

---

## Тестування

Завдяки патернам, код легко тестується:

```csharp
// Mock parser
public class MockXmlParser : IXmlParser
{
    public List<Book> ParseBooks(string xmlFilePath, SearchCriteria? criteria = null)
    {
        return new List<Book> { /* test data */ };
    }
}

// Test
var context = new XmlParserContext(new MockXmlParser());
var books = context.ParseBooks("test.xml");
Assert.AreEqual(expectedCount, books.Count);
```

---

## Висновок

Використання патернів проектування дозволило:
1. Створити гнучку та розширювану архітектуру
2. Спростити тестування
3. Поліпшити читабельність коду
4. Відокремити відповідальності
5. Дотриматись принципів SOLID
6. Уникнути anti-patterns
