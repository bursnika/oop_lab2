using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryXmlProcessor.Models;
using LibraryXmlProcessor.Services;
using LibraryXmlProcessor.Services.FileExporters;
using LibraryXmlProcessor.Services.GoogleDrive;
using LibraryXmlProcessor.Services.Logging;
using LibraryXmlProcessor.Services.Parsers;

namespace LibraryXmlProcessor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private XmlParserContext _parserContext;
    private readonly EventLogger _logger = EventLogger.Instance;
    private readonly GoogleDriveService _googleDriveService = new();
    private readonly XsltTransformationService _transformationService = new();

    public Window? MainWindow { get; set; }

    [ObservableProperty]
    private string _xmlFilePath = string.Empty;

    [ObservableProperty]
    private string _xsltFilePath = string.Empty;

    [ObservableProperty]
    private string _outputText = "Виберіть XML файл для початку роботи...";

    [ObservableProperty]
    private ObservableCollection<string> _parserTypes = new()
    {
        "SAX API (XmlReader)",
        "DOM API",
        "LINQ to XML"
    };

    [ObservableProperty]
    private string _selectedParserType = "LINQ to XML";

    [ObservableProperty]
    private ObservableCollection<string> _entityTypes = new()
    {
        "Книги",
        "Читачі",
        "Видані книги"
    };

    [ObservableProperty]
    private string _selectedEntityType = "Книги";

    [ObservableProperty]
    private ObservableCollection<string> _availableAttributes = new();

    [ObservableProperty]
    private string? _selectedAttribute;

    [ObservableProperty]
    private string _filterValue = string.Empty;

    [ObservableProperty]
    private bool _isGoogleDriveConnected = false;

    public MainWindowViewModel()
    {
        _parserContext = new XmlParserContext(new LinqXmlParser());

        // Load default files on startup
        LoadDefaultFiles();

        // Initialize Google Drive in background
        InitializeGoogleDriveAsync();
    }

    private async void InitializeGoogleDriveAsync()
    {
        try
        {
            var credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.json");

            if (!File.Exists(credPath))
            {
                IsGoogleDriveConnected = false;
                _logger.Log(LogLevel.Saving, "Google Drive: credentials.json не знайдено");
                return;
            }

            IsGoogleDriveConnected = await _googleDriveService.InitializeAsync();

            if (IsGoogleDriveConnected)
            {
                _logger.Log(LogLevel.Saving, "Google Drive успішно підключено");
            }
            else
            {
                _logger.Log(LogLevel.Saving, "Google Drive: помилка ініціалізації");
            }
        }
        catch (Exception ex)
        {
            IsGoogleDriveConnected = false;
            _logger.Log(LogLevel.Saving, $"Google Drive: {ex.Message}");
        }
    }

    private void LoadDefaultFiles()
    {
        // Load default XML file
        var defaultXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "library.xml");
        if (File.Exists(defaultXmlPath))
        {
            XmlFilePath = defaultXmlPath;
            OutputText = $"✅ Завантажено XML файл за замовчуванням: {Path.GetFileName(XmlFilePath)}\n📂 Шлях: {XmlFilePath}\n\n💡 Виберіть тип сутності та натисніть 'Пошук' для перегляду даних";
            LoadAvailableAttributes();
        }

        // Load default XSLT file
        var defaultXsltPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "library.xslt");
        if (File.Exists(defaultXsltPath))
        {
            XsltFilePath = defaultXsltPath;
        }
    }

    [RelayCommand]
    private async Task SelectXmlFile()
    {
        try
        {
            if (MainWindow?.StorageProvider == null)
            {
                // Fallback to default file
                var defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "library.xml");
                if (File.Exists(defaultPath))
                {
                    XmlFilePath = defaultPath;
                    OutputText = $"Вибрано XML файл: {Path.GetFileName(XmlFilePath)}";
                    LoadAvailableAttributes();
                }
                return;
            }

            var filePickerOptions = new FilePickerOpenOptions
            {
                Title = "Оберіть XML файл",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("XML Files")
                    {
                        Patterns = new[] { "*.xml" }
                    },
                    new FilePickerFileType("All Files")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            };

            var result = await MainWindow.StorageProvider.OpenFilePickerAsync(filePickerOptions);

            if (result.Count > 0)
            {
                XmlFilePath = result[0].Path.LocalPath;
                OutputText = $"✅ Вибрано XML файл: {Path.GetFileName(XmlFilePath)}\n📂 Шлях: {XmlFilePath}";
                LoadAvailableAttributes();
                _logger.Log(LogLevel.Filtering, $"Завантажено XML файл: {Path.GetFileName(XmlFilePath)}");
            }
        }
        catch (Exception ex)
        {
            OutputText = $"❌ Помилка вибору файлу: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SelectXsltFile()
    {
        try
        {
            if (MainWindow?.StorageProvider == null)
            {
                // Fallback to default file
                var defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "library.xslt");
                if (File.Exists(defaultPath))
                {
                    XsltFilePath = defaultPath;
                    OutputText = $"Вибрано XSLT файл: {Path.GetFileName(XsltFilePath)}";
                }
                return;
            }

            var filePickerOptions = new FilePickerOpenOptions
            {
                Title = "Оберіть XSLT файл",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("XSLT Files")
                    {
                        Patterns = new[] { "*.xslt", "*.xsl" }
                    },
                    new FilePickerFileType("All Files")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            };

            var result = await MainWindow.StorageProvider.OpenFilePickerAsync(filePickerOptions);

            if (result.Count > 0)
            {
                XsltFilePath = result[0].Path.LocalPath;
                OutputText = $"✅ Вибрано XSLT файл: {Path.GetFileName(XsltFilePath)}\n📂 Шлях: {XsltFilePath}";
                _logger.Log(LogLevel.Transformation, $"Завантажено XSLT файл: {Path.GetFileName(XsltFilePath)}");
            }
        }
        catch (Exception ex)
        {
            OutputText = $"❌ Помилка вибору файлу: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrEmpty(XmlFilePath) || !File.Exists(XmlFilePath))
        {
            OutputText = "❌ Спочатку оберіть XML файл!";
            return;
        }

        try
        {
            OutputText = "⏳ Починаю пошук...\n";
            UpdateParser();

            var criteria = new SearchCriteria();
            if (!string.IsNullOrWhiteSpace(FilterValue) && !string.IsNullOrWhiteSpace(SelectedAttribute))
            {
                criteria.AddFilter(SelectedAttribute, FilterValue);
            }

            var results = new List<string>();

            OutputText += $"📋 Тип сутності: {SelectedEntityType}\n";
            OutputText += $"⚙️ Парсер: {_parserContext.GetCurrentParser().GetParserName()}\n";
            OutputText += $"🔍 Фільтр: {criteria}\n\n";

            switch (SelectedEntityType)
            {
                case "Книги":
                    var books = _parserContext.ParseBooks(XmlFilePath, criteria.HasFilters ? criteria : null);
                    OutputText += $"📚 Знайдено книг: {books.Count}\n\n";
                    if (books.Count > 0)
                    {
                        results.AddRange(books.Select((b, index) => $"{index + 1}. {b.ToString()}"));
                    }
                    else
                    {
                        results.Add("Книг не знайдено за вказаними критеріями");
                    }
                    break;

                case "Читачі":
                    var readers = _parserContext.ParseReaders(XmlFilePath, criteria.HasFilters ? criteria : null);
                    OutputText += $"👥 Знайдено читачів: {readers.Count}\n\n";
                    if (readers.Count > 0)
                    {
                        results.AddRange(readers.Select((r, index) => $"{index + 1}. {r.ToString()}"));
                    }
                    else
                    {
                        results.Add("Читачів не знайдено за вказаними критеріями");
                    }
                    break;

                case "Видані книги":
                    var borrows = _parserContext.ParseBorrowedBooks(XmlFilePath, criteria.HasFilters ? criteria : null);
                    OutputText += $"📖 Знайдено записів про видачу: {borrows.Count}\n\n";
                    if (borrows.Count > 0)
                    {
                        results.AddRange(borrows.Select((b, index) => $"{index + 1}. {b.ToString()}"));
                    }
                    else
                    {
                        results.Add("Записів про видачу не знайдено за вказаними критеріями");
                    }
                    break;

                default:
                    OutputText += "❌ Невідомий тип сутності!\n";
                    return;
            }

            OutputText += string.Join("\n", results);

            _logger.Log(LogLevel.Filtering, $"Знайдено: {results.Count}, Параметри: {criteria}");
        }
        catch (Exception ex)
        {
            OutputText = $"❌ Помилка пошуку: {ex.Message}\n\n📋 Деталі:\n{ex.StackTrace}";
            _logger.Log(LogLevel.Filtering, $"Помилка пошуку: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task TransformToHtml()
    {
        if (string.IsNullOrEmpty(XmlFilePath) || !File.Exists(XmlFilePath))
        {
            OutputText = "Спочатку оберіть XML файл!";
            return;
        }

        if (string.IsNullOrEmpty(XsltFilePath) || !File.Exists(XsltFilePath))
        {
            OutputText = "Спочатку оберіть XSLT файл!";
            return;
        }

        try
        {
            var fileName = $"library_transformed_{DateTime.Now:yyyyMMdd_HHmmss}";
            var outputPath = await _transformationService.TransformToHtmlAsync(XmlFilePath, XsltFilePath, fileName);

            OutputText = $"HTML файл успішно створено:\n{outputPath}";
        }
        catch (Exception ex)
        {
            OutputText = $"Помилка трансформації: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportToXml()
    {
        if (string.IsNullOrEmpty(XmlFilePath) || !File.Exists(XmlFilePath))
        {
            OutputText = "Спочатку оберіть XML файл!";
            return;
        }

        try
        {
            var factory = new XmlExporterFactory();
            factory.LoadXmlFile(XmlFilePath);

            var fileName = $"library_filtered_{DateTime.Now:yyyyMMdd_HHmmss}";
            var outputPath = await factory.ExportAsync(fileName);

            _logger.Log(LogLevel.Saving, $"Збережено відфільтрований фрагмент у файл {Path.GetFileName(outputPath)}");
            OutputText = $"XML файл успішно збережено:\n{outputPath}";
        }
        catch (Exception ex)
        {
            OutputText = $"Помилка експорту: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportToHtml()
    {
        if (string.IsNullOrEmpty(XmlFilePath) || !File.Exists(XmlFilePath))
        {
            OutputText = "Спочатку оберіть XML файл!";
            return;
        }

        if (string.IsNullOrEmpty(XsltFilePath) || !File.Exists(XsltFilePath))
        {
            var defaultXslt = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "library.xslt");
            if (File.Exists(defaultXslt))
            {
                XsltFilePath = defaultXslt;
            }
            else
            {
                OutputText = "Спочатку оберіть XSLT файл!";
                return;
            }
        }

        try
        {
            var factory = new HtmlExporterFactory(XsltFilePath);
            factory.LoadXmlFile(XmlFilePath);

            var fileName = $"library_export_{DateTime.Now:yyyyMMdd_HHmmss}";
            var outputPath = await factory.ExportAsync(fileName);

            _logger.Log(LogLevel.Saving, $"Збережено відфільтрований фрагмент у файл {Path.GetFileName(outputPath)}");
            OutputText = $"HTML файл успішно збережено:\n{outputPath}";
        }
        catch (Exception ex)
        {
            OutputText = $"Помилка експорту: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task UploadToGoogleDrive()
    {
        if (!IsGoogleDriveConnected)
        {
            var credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.json");
            var credExists = File.Exists(credPath);

            OutputText = $"❌ Google Drive не підключено\n\n";
            OutputText += $"📋 Статус credentials.json: {(credExists ? "✅ Знайдено" : "❌ Не знайдено")}\n";
            OutputText += $"📂 Шлях: {credPath}\n\n";

            if (!credExists)
            {
                OutputText += "💡 Інструкція:\n";
                OutputText += "1. Створіть проект в Google Cloud Console\n";
                OutputText += "2. Увімкніть Google Drive API\n";
                OutputText += "3. Створіть OAuth 2.0 Client ID credentials\n";
                OutputText += "4. Завантажте credentials.json файл\n";
                OutputText += "5. Помістіть його в корінь проекту\n";
                OutputText += "6. Перезапустіть програму";
            }
            else
            {
                OutputText += "💡 Спробуйте перезапустити програму для повторної ініціалізації Google Drive";
            }

            return;
        }

        if (string.IsNullOrEmpty(XmlFilePath) || !File.Exists(XmlFilePath))
        {
            OutputText = "❌ Спочатку оберіть XML файл!";
            return;
        }

        try
        {
            OutputText = $"⏳ Завантаження файлу на Google Drive...\n";
            OutputText += $"📄 Файл: {Path.GetFileName(XmlFilePath)}\n";
            OutputText += $"📦 Розмір: {new FileInfo(XmlFilePath).Length / 1024.0:F2} KB\n\n";

            var fileId = await _googleDriveService.UploadFileAsync(XmlFilePath);

            if (fileId != null)
            {
                OutputText += $"✅ Файл успішно завантажено на Google Drive!\n\n";
                OutputText += $"🆔 File ID: {fileId}\n";
                OutputText += $"📅 Час завантаження: {DateTime.Now:dd.MM.yyyy HH:mm:ss}\n\n";
                OutputText += $"💡 Файл доступний у вашому Google Drive";

                _logger.Log(LogLevel.Saving, $"Завантажено на Google Drive: {Path.GetFileName(XmlFilePath)} (ID: {fileId})");
            }
            else
            {
                OutputText += "❌ Помилка завантаження на Google Drive";
            }
        }
        catch (Exception ex)
        {
            OutputText = $"❌ Помилка завантаження на Google Drive:\n{ex.Message}\n\n📋 Деталі:\n{ex.StackTrace}";
            _logger.Log(LogLevel.Saving, $"Помилка Google Drive: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Clear()
    {
        FilterValue = string.Empty;
        SelectedAttribute = null;
        OutputText = "Параметри пошуку очищено.";
    }

    [RelayCommand]
    private void OpenLogFile()
    {
        try
        {
            var logPath = _logger.GetLogFilePath();
            if (File.Exists(logPath))
            {
                var logContent = File.ReadAllText(logPath);
                OutputText = $"=== Лог файл ===\n{logPath}\n\n{logContent}";
            }
            else
            {
                OutputText = "Лог файл ще не створено.";
            }
        }
        catch (Exception ex)
        {
            OutputText = $"Помилка відкриття лог файлу: {ex.Message}";
        }
    }

    private void UpdateParser()
    {
        var parser = SelectedParserType switch
        {
            "SAX API (XmlReader)" => new SaxXmlParser() as IXmlParser,
            "DOM API" => new DomXmlParser(),
            "LINQ to XML" => new LinqXmlParser(),
            _ => new LinqXmlParser()
        };

        _parserContext.SetParser(parser);
    }

    private void LoadAvailableAttributes()
    {
        AvailableAttributes.Clear();

        var attributes = SelectedEntityType switch
        {
            "Книги" => new[] { "id", "isbn", "year", "available", "language", "edition", "Title", "Category", "Publisher" },
            "Читачі" => new[] { "id", "status", "membershipType", "Faculty", "Department", "Position" },
            "Видані книги" => new[] { "borrowId", "readerId", "bookId", "status" },
            _ => Array.Empty<string>()
        };

        foreach (var attr in attributes)
        {
            AvailableAttributes.Add(attr);
        }

        if (AvailableAttributes.Any())
        {
            SelectedAttribute = AvailableAttributes[0];
        }
    }

    partial void OnSelectedEntityTypeChanged(string value)
    {
        LoadAvailableAttributes();
    }
}
