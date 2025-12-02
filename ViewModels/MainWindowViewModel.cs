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
    private ObservableCollection<string> _availableTags = new();

    [ObservableProperty]
    private string? _selectedTag;

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
            LoadAvailableTags();
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
                    LoadAvailableTags();
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
                LoadAvailableTags();
                _logger.Log(LogLevel.Filtering, $"Завантажено XML файл: {Path.GetFileName(XmlFilePath)}");
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

        if (string.IsNullOrEmpty(SelectedTag))
        {
            OutputText = "❌ Спочатку оберіть тег для пошуку!";
            return;
        }

        try
        {
            OutputText = "⏳ Починаю динамічний пошук...\n\n";

            var doc = System.Xml.Linq.XDocument.Load(XmlFilePath);
            var results = new List<string>();

            // Get all elements with the selected tag
            var elements = doc.Descendants(SelectedTag);

            // If attribute and filter value are specified, filter by attribute value
            if (!string.IsNullOrWhiteSpace(SelectedAttribute) && !string.IsNullOrWhiteSpace(FilterValue))
            {
                elements = elements.Where(e =>
                {
                    var attrValue = e.Attribute(SelectedAttribute)?.Value;
                    return attrValue != null && attrValue.Contains(FilterValue, StringComparison.OrdinalIgnoreCase);
                });

                OutputText += $"🔍 Пошук: Тег = <{SelectedTag}>, Атрибут = {SelectedAttribute}, Значення = \"{FilterValue}\"\n\n";
            }
            else
            {
                OutputText += $"🔍 Пошук: Всі елементи <{SelectedTag}>\n\n";
            }

            var elementsList = elements.ToList();
            OutputText += $"📊 Знайдено елементів: {elementsList.Count}\n\n";

            if (elementsList.Count > 0)
            {
                foreach (var (element, index) in elementsList.Select((e, i) => (e, i)))
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    sb.AppendLine($"📌 Результат #{index + 1}");
                    sb.AppendLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                    // Show all attributes with beautiful formatting
                    foreach (var attr in element.Attributes())
                    {
                        var attrName = GetBeautifulAttributeName(attr.Name.LocalName);
                        sb.AppendLine($"   {attrName}: {attr.Value}");
                    }

                    // Show child elements (first level only)
                    if (element.HasElements)
                    {
                        sb.AppendLine();
                        foreach (var child in element.Elements())
                        {
                            var childValue = child.Value.Trim();
                            if (childValue.Length > 100)
                            {
                                childValue = childValue.Substring(0, 100) + "...";
                            }
                            var childName = GetBeautifulAttributeName(child.Name.LocalName);
                            sb.AppendLine($"   {childName}: {childValue}");
                        }
                    }

                    results.Add(sb.ToString());
                }

                OutputText += string.Join("\n", results);
            }
            else
            {
                OutputText += "❌ Елементів не знайдено за вказаними критеріями";
            }

            _logger.Log(LogLevel.Filtering, $"Динамічний пошук: Тег={SelectedTag}, Атрибут={SelectedAttribute}, Знайдено={elementsList.Count}");
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

        try
        {
            var xsltPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "library.xslt");
            if (!File.Exists(xsltPath))
            {
                OutputText = "❌ XSLT файл не знайдено!";
                return;
            }

            var fileName = $"library_transformed_{DateTime.Now:yyyyMMdd_HHmmss}";
            var outputPath = await _transformationService.TransformToHtmlAsync(XmlFilePath, xsltPath, fileName);

            OutputText = $"✅ HTML файл успішно створено:\n📂 {outputPath}";
            _logger.Log(LogLevel.Transformation, $"Створено HTML файл: {Path.GetFileName(outputPath)}");
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

        try
        {
            var xsltPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "library.xslt");
            if (!File.Exists(xsltPath))
            {
                OutputText = "❌ XSLT файл не знайдено!";
                return;
            }

            var factory = new HtmlExporterFactory(xsltPath);
            factory.LoadXmlFile(XmlFilePath);

            var fileName = $"library_export_{DateTime.Now:yyyyMMdd_HHmmss}";
            var outputPath = await factory.ExportAsync(fileName);

            _logger.Log(LogLevel.Saving, $"Збережено відфільтрований фрагмент у файл {Path.GetFileName(outputPath)}");
            OutputText = $"✅ HTML файл успішно збережено:\n📂 {outputPath}";
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

        try
        {
            // Open Google Drive dialog
            var dialogVm = new GoogleDriveDialogViewModel(_googleDriveService, XmlFilePath);
            var dialog = new Views.GoogleDriveDialog
            {
                DataContext = dialogVm
            };
            dialogVm.SetDialogWindow(dialog);

            var result = await dialog.ShowDialog<bool?>(MainWindow!);

            if (result == true)
            {
                // File was downloaded from Google Drive
                if (dialogVm?.SelectedFilePath != null && File.Exists(dialogVm.SelectedFilePath))
                {
                    XmlFilePath = dialogVm.SelectedFilePath;
                    OutputText = $"✅ Файл завантажено з Google Drive!\n";
                    OutputText += $"📄 Файл: {Path.GetFileName(XmlFilePath)}\n";
                    OutputText += $"📂 Шлях: {XmlFilePath}\n\n";
                    OutputText += $"💡 Тепер ви можете працювати з цим файлом";

                    LoadAvailableTags();
                    _logger.Log(LogLevel.Saving, $"Завантажено з Google Drive: {Path.GetFileName(XmlFilePath)}");
                }
            }
        }
        catch (Exception ex)
        {
            OutputText = $"❌ Помилка роботи з Google Drive:\n{ex.Message}";
            _logger.Log(LogLevel.Saving, $"Помилка Google Drive: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Clear()
    {
        FilterValue = string.Empty;
        SelectedTag = null;
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

    private void LoadAvailableTags()
    {
        AvailableTags.Clear();
        AvailableAttributes.Clear();

        if (string.IsNullOrEmpty(XmlFilePath) || !File.Exists(XmlFilePath))
        {
            return;
        }

        try
        {
            // Use LINQ to XML for dynamic tag discovery from entire document
            var doc = System.Xml.Linq.XDocument.Load(XmlFilePath);

            // Get all unique element names (tags) from the entire XML document
            var tags = doc.Descendants()
                .Select(e => e.Name.LocalName)
                .Where(name => name != "Library") // Exclude root element
                .Distinct()
                .OrderBy(t => t);

            foreach (var tag in tags)
            {
                AvailableTags.Add(tag);
            }

            if (AvailableTags.Any())
            {
                SelectedTag = AvailableTags[0];
            }

            _logger.Log(LogLevel.Filtering, $"Завантажено {AvailableTags.Count} тегів з XML файлу");
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Filtering, $"Помилка завантаження тегів: {ex.Message}");
        }
    }

    private void LoadAttributesForSelectedTag()
    {
        AvailableAttributes.Clear();

        if (string.IsNullOrEmpty(XmlFilePath) || !File.Exists(XmlFilePath) || string.IsNullOrEmpty(SelectedTag))
        {
            return;
        }

        try
        {
            // Use LINQ to XML for dynamic attribute discovery from entire document
            var doc = System.Xml.Linq.XDocument.Load(XmlFilePath);

            // Find the first element with the selected tag name anywhere in the document
            var firstElement = doc.Descendants(SelectedTag).FirstOrDefault();

            if (firstElement != null)
            {
                // Get all attribute names from this element
                var attributes = firstElement.Attributes()
                    .Select(a => a.Name.LocalName)
                    .OrderBy(a => a);

                foreach (var attr in attributes)
                {
                    AvailableAttributes.Add(attr);
                }

                if (AvailableAttributes.Any())
                {
                    SelectedAttribute = AvailableAttributes[0];
                }
            }

            _logger.Log(LogLevel.Filtering, $"Завантажено {AvailableAttributes.Count} атрибутів для тега {SelectedTag}");
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Filtering, $"Помилка завантаження атрибутів: {ex.Message}");
        }
    }

    partial void OnSelectedTagChanged(string? value)
    {
        LoadAttributesForSelectedTag();
    }

    private string GetBeautifulAttributeName(string attributeName)
    {
        return attributeName switch
        {
            "id" => "🆔 ID",
            "title" => "📖 Назва",
            "author" => "✍️ Автор",
            "reader" => "👤 Читач",
            "year" => "📅 Рік",
            "category" => "📂 Категорія",
            "isbn" => "📚 ISBN",
            "fullName" => "👤 Повне ім'я",
            "faculty" => "🎓 Факультет",
            "course" => "📊 Курс",
            "email" => "📧 Email",
            _ => $"📌 {attributeName}"
        };
    }
}
