using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using LibraryXmlProcessor.Services.Logging;

namespace LibraryXmlProcessor.Services.GoogleDrive;

public class GoogleDriveService
{
    private static readonly string[] Scopes = { DriveService.Scope.DriveFile };
    private const string ApplicationName = "Library XML Processor";
    private DriveService? _driveService;
    private readonly EventLogger _logger = EventLogger.Instance;

    public async Task<bool> InitializeAsync()
    {
        try
        {
            var credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.json");

            if (!File.Exists(credPath))
            {
                _logger.Log(LogLevel.Saving, $"credentials.json не знайдено за шляхом: {credPath}");
                return false;
            }

            _logger.Log(LogLevel.Saving, "Починається ініціалізація Google Drive...");

            UserCredential credential;
            using (var stream = new FileStream(credPath, FileMode.Open, FileAccess.Read))
            {
                var tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.json");

                _logger.Log(LogLevel.Saving, "Авторизація користувача через OAuth 2.0...");

                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(tokenPath, true));
            }

            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            _logger.Log(LogLevel.Saving, "Google Drive успішно ініціалізовано!");

            // Test connection
            try
            {
                var aboutRequest = _driveService.About.Get();
                aboutRequest.Fields = "user";
                var about = await aboutRequest.ExecuteAsync();
                _logger.Log(LogLevel.Saving, $"Підключено як: {about.User.EmailAddress}");
            }
            catch
            {
                // Ignore test error
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Saving, $"Критична помилка ініціалізації Google Drive: {ex.Message}");
            _logger.Log(LogLevel.Saving, $"Стек-трейс: {ex.StackTrace}");
            return false;
        }
    }

    public async Task<string?> UploadFileAsync(string filePath, string? folderId = null)
    {
        if (_driveService == null)
        {
            throw new InvalidOperationException("Google Drive service not initialized");
        }

        var fileName = Path.GetFileName(filePath);
        var mimeType = GetMimeType(filePath);

        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = fileName,
            Parents = folderId != null ? new List<string> { folderId } : null
        };

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var request = _driveService.Files.Create(fileMetadata, stream, mimeType);
        request.Fields = "id, name, webViewLink";

        var file = await request.UploadAsync();

        if (file.Status == Google.Apis.Upload.UploadStatus.Completed)
        {
            var uploadedFile = request.ResponseBody;
            _logger.Log(LogLevel.Saving, $"Файл {fileName} завантажено на Google Drive (ID: {uploadedFile.Id})");
            return uploadedFile.Id;
        }

        return null;
    }

    public async Task<string?> DownloadFileAsync(string fileId, string outputPath)
    {
        if (_driveService == null)
        {
            throw new InvalidOperationException("Google Drive service not initialized");
        }

        try
        {
            var request = _driveService.Files.Get(fileId);
            var stream = new MemoryStream();

            await request.DownloadAsync(stream);

            using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            stream.Seek(0, SeekOrigin.Begin);
            await stream.CopyToAsync(fileStream);

            _logger.Log(LogLevel.Saving, $"Файл завантажено з Google Drive: {outputPath}");
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Saving, $"Помилка завантаження файлу: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Google.Apis.Drive.v3.Data.File>> ListFilesAsync(string? folderId = null, string? query = null)
    {
        if (_driveService == null)
        {
            throw new InvalidOperationException("Google Drive service not initialized");
        }

        var files = new List<Google.Apis.Drive.v3.Data.File>();

        var listRequest = _driveService.Files.List();
        listRequest.PageSize = 100;
        listRequest.Fields = "files(id, name, mimeType, size, createdTime, modifiedTime)";

        if (!string.IsNullOrEmpty(query))
        {
            listRequest.Q = query;
        }
        else if (!string.IsNullOrEmpty(folderId))
        {
            listRequest.Q = $"'{folderId}' in parents";
        }

        var result = await listRequest.ExecuteAsync();
        files.AddRange(result.Files);

        return files;
    }

    public async Task<bool> DeleteFileAsync(string fileId)
    {
        if (_driveService == null)
        {
            throw new InvalidOperationException("Google Drive service not initialized");
        }

        try
        {
            await _driveService.Files.Delete(fileId).ExecuteAsync();
            _logger.Log(LogLevel.Saving, $"Файл видалено з Google Drive (ID: {fileId})");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Saving, $"Помилка видалення файлу: {ex.Message}");
            return false;
        }
    }

    public bool IsInitialized => _driveService != null;

    private string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".xml" => "application/xml",
            ".html" => "text/html",
            ".xslt" => "application/xslt+xml",
            ".txt" => "text/plain",
            ".json" => "application/json",
            _ => "application/octet-stream"
        };
    }
}
