# Налаштування Google Drive API

Це детальна інструкція як налаштувати інтеграцію з Google Drive для лабораторної роботи.

## Крок 1: Створення проекту в Google Cloud Console

1. Перейдіть на [Google Cloud Console](https://console.cloud.google.com/)
2. Увійдіть з вашим Google акаунтом
3. Натисніть на розкривний список проектів (зверху зліва)
4. Натисніть "NEW PROJECT"
5. Введіть назву проекту (наприклад, "Library XML Processor")
6. Натисніть "CREATE"

## Крок 2: Увімкнення Google Drive API

1. У меню зліва оберіть "APIs & Services" > "Library"
2. У пошуку введіть "Google Drive API"
3. Натисніть на "Google Drive API"
4. Натисніть "ENABLE"

## Крок 3: Створення OAuth 2.0 Credentials

1. У меню зліва оберіть "APIs & Services" > "Credentials"
2. Натисніть "CREATE CREDENTIALS" > "OAuth client ID"
3. Якщо з'явиться повідомлення про OAuth consent screen:
   - Натисніть "CONFIGURE CONSENT SCREEN"
   - Оберіть "External" (якщо не маєте Google Workspace)
   - Натисніть "CREATE"
   - Заповніть обов'язкові поля:
     - App name: "Library XML Processor"
     - User support email: ваш email
     - Developer contact information: ваш email
   - Натисніть "SAVE AND CONTINUE"
   - На сторінці "Scopes" натисніть "SAVE AND CONTINUE"
   - На сторінці "Test users" додайте ваш email та натисніть "SAVE AND CONTINUE"
   - Натисніть "BACK TO DASHBOARD"

4. Поверніться до "Credentials" та знову натисніть "CREATE CREDENTIALS" > "OAuth client ID"
5. Оберіть "Desktop app" як Application type
6. Введіть назву (наприклад, "Desktop Client")
7. Натисніть "CREATE"

## Крок 4: Завантаження credentials.json

1. У списку OAuth 2.0 Client IDs знайдіть щойно створений клієнт
2. Натисніть на іконку завантаження (Download JSON) справа
3. Збережіть файл
4. Перейменуйте файл на `credentials.json`
5. Розмістіть файл в кореневій директорії проекту (поруч з LibraryXmlProcessor.csproj)

## Крок 5: Перша автентифікація

1. Запустіть застосунок:
   ```bash
   dotnet run
   ```

2. При першому запуску:
   - Відкриється браузер з запитом на дозвіл
   - Увійдіть з вашим Google акаунтом
   - Натисніть "Allow"
   - Браузер покаже повідомлення що автентифікація успішна
   - Повернітесь до застосунку

3. Токен автентифікації збережеться в файлі `token.json` (автоматично)

## Структура credentials.json

```json
{
  "installed": {
    "client_id": "ВАШ_CLIENT_ID.apps.googleusercontent.com",
    "project_id": "ваш-проект-id",
    "auth_uri": "https://accounts.google.com/o/oauth2/auth",
    "token_uri": "https://oauth2.googleapis.com/token",
    "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
    "client_secret": "ВАШ_CLIENT_SECRET",
    "redirect_uris": [
      "http://localhost"
    ]
  }
}
```

## Використання в застосунку

Після налаштування, ви зможете:

1. **Завантажувати файли на Google Drive**:
   - Оберіть XML файл
   - Натисніть кнопку "Google Drive"
   - Файл буде завантажено на ваш Google Drive
   - У виводі з'явиться File ID

2. **Перевірити статус підключення**:
   - У заголовку вікна відображається "Google Drive: True/False"
   - True означає успішне підключення

## Troubleshooting

### Помилка "credentials.json not found"
- Переконайтесь що файл знаходиться в кореневій директорії проекту
- Переконайтесь що файл називається саме `credentials.json`

### Помилка "Access not configured"
- Переконайтесь що Google Drive API увімкнено в консолі
- Зачекайте кілька хвилин після увімкнення API

### Браузер не відкривається
- Скопіюйте URL з консолі та відкрийте вручну
- Переконайтесь що порт не заблокований

### Помилка "redirect_uri_mismatch"
- Переконайтесь що ви обрали "Desktop app" при створенні credentials
- Додайте `http://localhost` до списку redirect URIs

## Безпека

**ВАЖЛИВО**:
- НЕ додавайте `credentials.json` до Git репозиторію
- НЕ діліться файлом `credentials.json` з іншими
- НЕ публікуйте Client Secret
- Файл вже додано до `.gitignore`

## Видалення доступу

Якщо потрібно відкликати доступ:

1. Перейдіть на [Google Account](https://myaccount.google.com/)
2. Оберіть "Security" > "Third-party apps with account access"
3. Знайдіть "Library XML Processor"
4. Натисніть "Remove Access"

## Обмеження

- **Квоти**: Безкоштовний акаунт має обмеження на кількість запитів
- **Розмір файлів**: Стандартне обмеження Google Drive
- **Швидкість**: Залежить від швидкості інтернету

## Корисні посилання

- [Google Cloud Console](https://console.cloud.google.com/)
- [Google Drive API Documentation](https://developers.google.com/drive/api/v3/about-sdk)
- [OAuth 2.0 Guide](https://developers.google.com/identity/protocols/oauth2)
