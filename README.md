# Лабораторна робота №2 - Робота з файлами XML

## Кафедральна бібліотека - XML процесор

Avalonia застосунок для обробки XML файлів з даними бібліотеки з використанням патернів проектування.

## Реалізовані патерни проектування

### 1. Strategy (Стратегія)
Реалізовано три способи парсингу XML:
- **SAX API** (через XmlReader) - event-driven парсинг
- **DOM API** (через XmlDocument) - повне завантаження дерева в пам'ять
- **LINQ to XML** (через XDocument) - сучасний підхід з LINQ запитами

Файли:
- [Services/Parsers/IXmlParser.cs](Services/Parsers/IXmlParser.cs) - інтерфейс стратегії
- [Services/Parsers/SaxXmlParser.cs](Services/Parsers/SaxXmlParser.cs) - SAX реалізація
- [Services/Parsers/DomXmlParser.cs](Services/Parsers/DomXmlParser.cs) - DOM реалізація
- [Services/Parsers/LinqXmlParser.cs](Services/Parsers/LinqXmlParser.cs) - LINQ реалізація
- [Services/Parsers/XmlParserContext.cs](Services/Parsers/XmlParserContext.cs) - контекст стратегії

### 2. Factory Method (Фабричний метод)
Реалізовано для збереження відфільтрованих даних у різних форматах:
- **XML формат** - пряме збереження
- **HTML формат** - через XSLT трансформацію

Файли:
- [Services/FileExporters/FileExporterBase.cs](Services/FileExporters/FileExporterBase.cs) - базовий клас з фабричним методом
- [Services/FileExporters/XmlFileExporter.cs](Services/FileExporters/XmlFileExporter.cs) - XML експортер
- [Services/FileExporters/HtmlFileExporter.cs](Services/FileExporters/HtmlFileExporter.cs) - HTML експортер

### 3. Singleton (Одинак)
Реалізовано для системи логування подій:
- Фіксація часу події
- 3 рівні важливості: Фільтрація, Трансформація, Збереження
- Потокобезпечна реалізація

Файли:
- [Services/Logging/EventLogger.cs](Services/Logging/EventLogger.cs)

## Структура проекту

```
LibraryXmlProcessor/
├── Data/                      # XML та XSLT файли
│   ├── library.xml           # Вхідний XML файл з даними бібліотеки
│   └── library.xslt          # XSLT для трансформації в HTML
├── Models/                    # Моделі даних
│   ├── Book.cs               # Модель книги та автора
│   ├── Reader.cs             # Модель читача
│   ├── BorrowedBook.cs       # Модель позики
│   └── SearchCriteria.cs     # Критерії пошуку
├── Services/                  # Бізнес-логіка
│   ├── Parsers/              # Strategy Pattern
│   ├── FileExporters/        # Factory Method Pattern
│   ├── Logging/              # Singleton Pattern
│   ├── GoogleDrive/          # Google Drive інтеграція
│   └── XsltTransformationService.cs
├── ViewModels/               # MVVM ViewModels
│   └── MainWindowViewModel.cs
└── Views/                    # Avalonia UI
    └── MainWindow.axaml
```

## Вимоги

- .NET 9.0 SDK
- Avalonia UI Framework
- Google APIs (для Google Drive функціоналу)

## Запуск проекту

1. Клонуйте репозиторій або розпакуйте архів
2. Відкрийте термінал в директорії проекту
3. Виконайте команду для збірки:
   ```bash
   dotnet build
   ```
4. Запустіть застосунок:
   ```bash
   dotnet run
   ```

## Функціональність

### Основні можливості:

1. **Вибір XML файлу** - завантаження даних бібліотеки
2. **Вибір XSLT файлу** - для трансформації
3. **Вибір парсера** - перемикання між SAX, DOM, LINQ to XML
4. **Вибір типу сутності** - Книги, Читачі, Видані книги
5. **Фільтрація даних** - пошук за атрибутами
6. **Трансформація в HTML** - через XSLT
7. **Експорт в XML** - збереження відфільтрованих даних
8. **Експорт в HTML** - збереження з трансформацією
9. **Завантаження на Google Drive** - інтеграція з хмарним сховищем
10. **Перегляд логів** - всі операції логуються
11. **Очистка фільтрів** - кнопка Clear
12. **Підтвердження виходу** - діалог при закритті програми

### Приклад використання:

1. Натисніть "Вибрати XML файл" (автоматично завантажить Data/library.xml)
2. Оберіть тип парсера (наприклад, "LINQ to XML")
3. Оберіть тип сутності (наприклад, "Книги")
4. Оберіть атрибут для фільтрації (наприклад, "Category")
5. Введіть значення фільтра (наприклад, "Software")
6. Натисніть "Пошук"
7. Результати відобразяться в текстовому полі

## XML структура

XML файл має 3-рівневу структуру:
- Library (кореневий елемент)
  - Books - колекція книг
    - Book - книга з атрибутами (id, isbn, year, available, language, edition)
  - Readers - колекція читачів
    - Reader - читач з атрибутами (id, registrationDate, status, membershipType)
  - BorrowedBooks - колекція позик
    - Borrow - позика з атрибутами (borrowId, readerId, bookId, borrowDate, dueDate, status, renewable)

**Важливо**: Різні вузли мають різну кількість атрібутів (згідно вимог завдання).

## Google Drive Integration

Для роботи з Google Drive потрібно:

1. Створити проект в [Google Cloud Console](https://console.cloud.google.com/)
2. Увімкнути Google Drive API
3. Створити OAuth 2.0 credentials
4. Завантажити credentials.json файл
5. Розмістити credentials.json в корені проекту

При першому запуску браузер відкриється для автентифікації.

## Логування

Всі події логуються в файл `Logs/library_log_YYYYMMDD.txt` з такою структурою:
```
DD.MM.YYYY HH:mm:ss Фільтрація. Параметри: attribute=value
DD.MM.YYYY HH:mm:ss Трансформація. Збережено у файл output.html
DD.MM.YYYY HH:mm:ss Збереження. Збережено відфільтрований фрагмент у файл data.xml
```

## Експорт файлів

Всі експортовані файли зберігаються в директорії `Exports/`:
- `library_filtered_YYYYMMDD_HHmmss.xml` - відфільтровані XML дані
- `library_export_YYYYMMDD_HHmmss.html` - експортовані HTML дані
- `library_transformed_YYYYMMDD_HHmmss.html` - трансформовані HTML файли

## Технології

- **Framework**: .NET 9.0
- **UI**: Avalonia 11.3.9
- **MVVM**: CommunityToolkit.Mvvm 8.2.1
- **Google APIs**: Google.Apis.Drive.v3, Google.Apis.Auth
- **XML обробка**: System.Xml, System.Xml.Linq, System.Xml.Xsl

## Автор

Лабораторна робота №2 з ООП
Тема: Кафедральна бібліотека

## Ліцензія

Educational purposes only
