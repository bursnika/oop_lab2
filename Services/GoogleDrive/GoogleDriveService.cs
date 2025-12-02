using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using LibraryXmlProcessor.Services.Logging;

namespace LibraryXmlProcessor.Services.GoogleDrive;

public class GoogleDriveService
{
    private static readonly string[] Scopes = { DriveService.Scope.Drive };
    private const string ApplicationName = "Library XML Processor";
    private DriveService? _driveService;
    private UserCredential? _credential;
    private readonly EventLogger _logger = EventLogger.Instance;

    private static string TokenPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LibraryXmlProcessor",
        "Google.Apis.Auth");

    public bool IsAuthenticated => _credential != null;

    public async Task<bool> InitializeAsync()
    {
        return await InitializeAsync(false);
    }

    public async Task<bool> InitializeAsync(bool forceReauth)
    {
        try
        {
            Console.WriteLine($"[AUTH] Starting authentication process. ForceReauth: {forceReauth}");
            _logger.Log(LogLevel.Saving, $"Початок автентифікації. ForceReauth: {forceReauth}");

            // Handle force re-authentication
            if (forceReauth && Directory.Exists(TokenPath))
            {
                _logger.Log(LogLevel.Saving, $"Видалення старих токенів з: {TokenPath}");
                try
                {
                    Directory.Delete(TokenPath, true);
                    _logger.Log(LogLevel.Saving, "Старі токени видалено успішно");
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Saving, $"Помилка видалення токенів: {ex.Message}");
                }
            }

            // Find credentials.json
            var credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.json");
            Console.WriteLine($"[AUTH] Checking for credentials at: {credPath}");

            if (!File.Exists(credPath))
            {
                Console.WriteLine($"[AUTH] ERROR: credentials.json not found at {credPath}");
                _logger.Log(LogLevel.Saving, $"credentials.json не знайдено за шляхом: {credPath}");
                return false;
            }

            Console.WriteLine("[AUTH] credentials.json found successfully");
            _logger.Log(LogLevel.Saving, "Починається ініціалізація Google Drive...");

            GoogleClientSecrets secrets;
            using (var stream = new FileStream(credPath, FileMode.Open, FileAccess.Read))
            {
                secrets = GoogleClientSecrets.FromStream(stream);
                Console.WriteLine($"[AUTH] OAuth ClientId: {secrets.Secrets.ClientId}");
                Console.WriteLine($"[AUTH] Token storage path: {TokenPath}");
                Console.WriteLine($"[AUTH] Requested scopes: {string.Join(", ", Scopes)}");

                _logger.Log(LogLevel.Saving, $"OAuth ClientId: {secrets.Secrets.ClientId}");
                _logger.Log(LogLevel.Saving, $"Token storage path: {TokenPath}");
                _logger.Log(LogLevel.Saving, $"Requested scopes: {string.Join(", ", Scopes)}");
            }

            // Create token storage directory if needed
            if (!Directory.Exists(TokenPath))
            {
                Directory.CreateDirectory(TokenPath);
                Console.WriteLine($"[AUTH] Created token directory: {TokenPath}");
            }

            var dataStore = new FileDataStore(TokenPath, true);

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = secrets.Secrets,
                Scopes = Scopes,
                DataStore = dataStore
            });

            Console.WriteLine("[AUTH] Starting OAuth authorization flow...");
            _logger.Log(LogLevel.Saving, "Запуск OAuth авторизації...");

            var token = await flow.LoadTokenAsync("user", CancellationToken.None);

            if (token == null || (forceReauth && token.IsStale))
            {
                Console.WriteLine("[AUTH] Token not found or expired. Starting browser authentication...");
                _logger.Log(LogLevel.Saving, "Токен не знайдено або застарів. Запуск автентифікації через браузер...");

                // Use GoogleWebAuthorizationBroker for simplicity
                _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets.Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    dataStore);

                Console.WriteLine("[AUTH] Browser authentication completed successfully");
                _logger.Log(LogLevel.Saving, "Автентифікація через браузер завершена успішно");
            }
            else
            {
                Console.WriteLine("[AUTH] Using existing valid token");
                _logger.Log(LogLevel.Saving, "Використання існуючого дійсного токена");
                _credential = new UserCredential(flow, "user", token);
            }

            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = _credential,
                ApplicationName = ApplicationName
            });

            Console.WriteLine("[AUTH] Google Drive service initialized successfully");
            _logger.Log(LogLevel.Saving, "Google Drive сервіс успішно ініціалізовано");

            // Test connection and get user info
            try
            {
                Console.WriteLine("[AUTH] Testing connection...");
                var aboutRequest = _driveService.About.Get();
                aboutRequest.Fields = "user";
                var about = await aboutRequest.ExecuteAsync();

                Console.WriteLine($"[AUTH] ✅ Connected as: {about.User.EmailAddress}");
                _logger.Log(LogLevel.Saving, $"✅ Підключено як: {about.User.EmailAddress}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUTH] Warning: Could not verify user info: {ex.Message}");
                _logger.Log(LogLevel.Saving, $"Попередження: Не вдалося перевірити інформацію користувача: {ex.Message}");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH] EXCEPTION: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[AUTH] Stack trace: {ex.StackTrace}");
            _logger.Log(LogLevel.Saving, $"Критична помилка ініціалізації Google Drive: {ex.Message}");
            _logger.Log(LogLevel.Saving, $"Тип помилки: {ex.GetType().Name}");
            _logger.Log(LogLevel.Saving, $"Стек-трейс: {ex.StackTrace}");
            return false;
        }
    }

    public async Task<string?> UploadFileAsync(string filePath, string? folderId = null)
    {
        if (_driveService == null)
        {
            Console.WriteLine("[UPLOAD] ERROR: Google Drive service not initialized");
            _logger.Log(LogLevel.Saving, "Помилка: Google Drive сервіс не ініціалізовано");
            throw new InvalidOperationException("Google Drive service not initialized");
        }

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[UPLOAD] ERROR: File not found: {filePath}");
            _logger.Log(LogLevel.Saving, $"Помилка: Файл не знайдено: {filePath}");
            throw new FileNotFoundException("File not found", filePath);
        }

        try
        {
            var fileName = Path.GetFileName(filePath);
            var mimeType = GetMimeType(filePath);
            var fileSize = new FileInfo(filePath).Length;

            Console.WriteLine($"[UPLOAD] Starting upload: {fileName}");
            Console.WriteLine($"[UPLOAD] File size: {fileSize / 1024.0:F2} KB");
            Console.WriteLine($"[UPLOAD] MIME type: {mimeType}");

            _logger.Log(LogLevel.Saving, $"Початок завантаження: {fileName}, розмір: {fileSize / 1024.0:F2} KB");

            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName,
                Parents = folderId != null ? new List<string> { folderId } : null
            };

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var request = _driveService.Files.Create(fileMetadata, stream, mimeType);
            request.Fields = "id, name, webViewLink";

            Console.WriteLine("[UPLOAD] Uploading to Google Drive...");
            var uploadResult = await request.UploadAsync();

            if (uploadResult.Status == Google.Apis.Upload.UploadStatus.Completed)
            {
                var uploadedFile = request.ResponseBody;
                Console.WriteLine($"[UPLOAD] ✅ Upload completed successfully!");
                Console.WriteLine($"[UPLOAD] File ID: {uploadedFile.Id}");
                Console.WriteLine($"[UPLOAD] Web view link: {uploadedFile.WebViewLink}");

                _logger.Log(LogLevel.Saving, $"✅ Файл {fileName} завантажено на Google Drive (ID: {uploadedFile.Id})");
                return uploadedFile.Id;
            }
            else
            {
                Console.WriteLine($"[UPLOAD] ERROR: Upload failed with status: {uploadResult.Status}");
                Console.WriteLine($"[UPLOAD] Exception: {uploadResult.Exception?.Message}");
                _logger.Log(LogLevel.Saving, $"Помилка завантаження: {uploadResult.Exception?.Message}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UPLOAD] EXCEPTION: {ex.GetType().Name}: {ex.Message}");
            _logger.Log(LogLevel.Saving, $"Помилка завантаження файлу: {ex.Message}");
            throw;
        }
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

    public void Logout()
    {
        Console.WriteLine("[AUTH] Logging out and disposing Google services");
        _logger.Log(LogLevel.Saving, "Вихід з Google Drive");

        _credential = null;
        _driveService?.Dispose();
        _driveService = null;

        Console.WriteLine("[AUTH] Logout completed");
        _logger.Log(LogLevel.Saving, "Вихід виконано успішно");
    }

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
