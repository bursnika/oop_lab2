using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryXmlProcessor.Services.GoogleDrive;
using LibraryXmlProcessor.Services.Logging;
using LibraryXmlProcessor.Services;

namespace LibraryXmlProcessor.ViewModels;

public partial class GoogleDriveDialogViewModel : ViewModelBase
{
    private readonly GoogleDriveService _googleDriveService;
    private readonly EventLogger _logger = EventLogger.Instance;
    private readonly XsltTransformationService _transformationService = new();
    private Window? _dialogWindow;

    [ObservableProperty]
    private ObservableCollection<GoogleDriveFileItem> _files = new();

    [ObservableProperty]
    private GoogleDriveFileItem? _selectedFile;

    [ObservableProperty]
    private string _statusText = "Завантаження списку файлів...";

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string _currentXmlPath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _uploadFormats = new() { "XML", "HTML", "XSLT" };

    [ObservableProperty]
    private string _selectedUploadFormat = "XML";

    public string? SelectedFilePath { get; private set; }
    public bool ShouldUpload { get; private set; }

    public GoogleDriveDialogViewModel(GoogleDriveService googleDriveService, string currentXmlPath)
    {
        _googleDriveService = googleDriveService;
        _currentXmlPath = currentXmlPath;

        LoadFilesAsync();
    }

    public void SetDialogWindow(Window dialogWindow)
    {
        _dialogWindow = dialogWindow;
    }

    private async void LoadFilesAsync()
    {
        try
        {
            IsLoading = true;
            StatusText = "⏳ Завантаження списку файлів з Google Drive...";

            var driveFiles = await _googleDriveService.ListFilesAsync(query: "(mimeType='application/xml' or mimeType='text/html' or mimeType='application/xslt+xml' or mimeType='text/xsl') and trashed=false");

            Files.Clear();

            if (driveFiles.Count == 0)
            {
                StatusText = "📭 XML, HTML та XSLT файлів не знайдено на Google Drive";
            }
            else
            {
                foreach (var file in driveFiles.OrderByDescending(f => f.ModifiedTimeDateTimeOffset))
                {
                    Files.Add(new GoogleDriveFileItem
                    {
                        Id = file.Id,
                        Name = file.Name,
                        ModifiedTime = file.ModifiedTimeDateTimeOffset,
                        Size = file.Size ?? 0,
                        WebViewLink = file.WebViewLink
                    });
                }

                StatusText = $"✅ Знайдено {Files.Count} файлів (XML, HTML, XSLT)";
            }

            _logger.Log(LogLevel.Saving, $"Завантажено список файлів з Google Drive: {Files.Count} файлів");
        }
        catch (Exception ex)
        {
            StatusText = $"❌ Помилка завантаження: {ex.Message}";
            _logger.Log(LogLevel.Saving, $"Помилка завантаження списку файлів: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DownloadFile()
    {
        if (SelectedFile == null)
        {
            StatusText = "❌ Виберіть файл для завантаження";
            return;
        }

        try
        {
            IsLoading = true;
            StatusText = $"⏳ Завантаження файлу {SelectedFile.Name}...";

            var tempPath = Path.Combine(Path.GetTempPath(), SelectedFile.Name);

            var result = await _googleDriveService.DownloadFileAsync(SelectedFile.Id, tempPath);

            if (result != null && File.Exists(result))
            {
                SelectedFilePath = result;
                StatusText = $"✅ Файл завантажено: {SelectedFile.Name}";
                _logger.Log(LogLevel.Saving, $"Завантажено з Google Drive: {SelectedFile.Name}");

                // If HTML file, open in browser
                if (Path.GetExtension(SelectedFile.Name).Equals(".html", StringComparison.OrdinalIgnoreCase))
                {
                    OpenInBrowser(result);
                    StatusText += " (відкрито в браузері)";
                }

                // Close dialog after successful download
                await Task.Delay(500);
                _dialogWindow?.Close(true);
            }
            else
            {
                StatusText = "❌ Помилка завантаження файлу";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"❌ Помилка: {ex.Message}";
            _logger.Log(LogLevel.Saving, $"Помилка завантаження файлу: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenInBrowser(string filePath)
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                }
            };
            process.Start();
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Saving, $"Помилка відкриття HTML файлу: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task UploadCurrentFile()
    {
        if (string.IsNullOrEmpty(CurrentXmlPath) || !File.Exists(CurrentXmlPath))
        {
            StatusText = "❌ Немає файлу для завантаження";
            return;
        }

        try
        {
            IsLoading = true;
            string fileToUpload = CurrentXmlPath;
            var baseFileName = Path.GetFileNameWithoutExtension(CurrentXmlPath);

            // Handle different upload formats
            if (SelectedUploadFormat == "XSLT")
            {
                // Upload XSLT file
                var xsltPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "library.xslt");
                if (!File.Exists(xsltPath))
                {
                    StatusText = "❌ XSLT файл не знайдено";
                    return;
                }
                fileToUpload = xsltPath;
                StatusText = $"⏳ Завантаження {Path.GetFileName(fileToUpload)} на Google Drive...";
            }
            else if (SelectedUploadFormat == "HTML")
            {
                StatusText = "⏳ Трансформація XML в HTML...";

                var xsltPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "library.xslt");
                if (!File.Exists(xsltPath))
                {
                    StatusText = "❌ XSLT файл не знайдено";
                    return;
                }

                var htmlFileName = $"{baseFileName}_{DateTime.Now:yyyyMMdd_HHmmss}";
                var htmlPath = await _transformationService.TransformToHtmlAsync(CurrentXmlPath, xsltPath, htmlFileName);

                if (File.Exists(htmlPath))
                {
                    fileToUpload = htmlPath;
                    StatusText = $"⏳ Завантаження {Path.GetFileName(htmlPath)} на Google Drive...";
                }
                else
                {
                    StatusText = "❌ Помилка трансформації в HTML";
                    return;
                }
            }
            else
            {
                // XML format
                StatusText = $"⏳ Завантаження {Path.GetFileName(fileToUpload)} на Google Drive...";
            }

            var fileId = await _googleDriveService.UploadFileAsync(fileToUpload);

            if (fileId != null)
            {
                StatusText = $"✅ Файл успішно завантажено!";
                _logger.Log(LogLevel.Saving, $"Завантажено на Google Drive: {Path.GetFileName(fileToUpload)}");

                // Refresh file list
                await Task.Delay(500);
                LoadFilesAsync();
            }
            else
            {
                StatusText = "❌ Помилка завантаження на Google Drive";
            }

            // Clean up temporary HTML file
            if (SelectedUploadFormat == "HTML" && fileToUpload != CurrentXmlPath && File.Exists(fileToUpload))
            {
                try { File.Delete(fileToUpload); } catch { }
            }
        }
        catch (Exception ex)
        {
            StatusText = $"❌ Помилка: {ex.Message}";
            _logger.Log(LogLevel.Saving, $"Помилка завантаження: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadFilesAsync();
    }

    [RelayCommand]
    private void Close()
    {
        _dialogWindow?.Close(false);
    }
}

public class GoogleDriveFileItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset? ModifiedTime { get; set; }
    public long Size { get; set; }
    public string? WebViewLink { get; set; }

    public string DisplayName => Name;
    public string DisplayDate => ModifiedTime?.ToString("dd.MM.yyyy HH:mm") ?? "Невідомо";
    public string DisplaySize => Size > 0 ? $"{Size / 1024.0:F2} KB" : "0 KB";
}
