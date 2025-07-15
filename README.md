# Telegram Insurance Bot 💊

A smart Telegram bot built with .NET 8 using Clean Architecture + CQRS to process car insurance policy applications.
It uses Mindee OCR, OpenAI GPT, PostgreSQL, and generates a PDF-based insurance policy.

---

## ✅ Features

* Telegram bot interface using Webhooks
* Document upload: Passport & Car Registration
* OCR parsing using Mindee API
* GPT-based personalized message generation via OpenAI
* Insurance Policy generation as PDF
* Clean Architecture + CQRS
* Retry, cancellation, admin summary/logs
* Dockerized deployment support

---

## 🌐 Tech Stack

* **.NET 8**, **MediatR**, **EF Core**
* **Telegram.Bot** SDK
* **Mindee API** for OCR
* **OpenRouter API** for GPT
* **PostgreSQL** database
* **QuestPDF** for PDF generation
* **Docker**, **Docker Compose**

---

## 📁 Project Structure (Clean Architecture)

```
TelegramBot/             -> Entry point / Controllers
Application/             -> Handlers, Commands, Interfaces
Domain/                  -> Entities, Enums, 
Infrastructure/          -> Implementations (OCR, PDF, Storage)
Persistence/             -> DbContext, EF Repositories, UoW
Shared/                  -> Utilities, Constants
Application.Tests/       -> Unit tests for core logic
Infrastructure.Tests/    -> Unit tests for OCR, storage
```

---
## 🧠 Design Patterns Used
Pattern	Purpose
CQRS + MediatR	Separation of read/write responsibilities
Builder Pattern	Used in PdfPolicyBuilder to modularly build PDF
Repository Pattern	Encapsulation of EF Core queries
Service Abstractions	Testability and layer isolation
Factory (DbContextFactory)	Migration və test zamanı DbContext yaratmaq üçün
Middleware	Centralized error handling and logging

---

## 📃 CQRS Flow

* Each action is modeled as a `Command` or `Query`
* `Handlers` process logic (e.g., UploadDocumentCommandHandler)
* `IMediator` routes commands from the update dispatcher
* Flow controlled via `UserStep` enum and `UserStateService`

---

## 🔄 Sample Flow

1. `/start`
2. Upload Passport -> OCR -> extract fields
3. Upload Car Doc -> OCR -> extract fields
4. Show parsed data to user
5. `confirm` -> show price
6. `confirm` again -> Generate PDF
7. PDF sent to user

---

## 👤 Admin Features

* `/adminsummary` - issued policies
* `/logs` - failed generations
* `/simulateocr` - inject test data

---

## 🕹️ Run with Docker

```bash
docker-compose up --build
```

Ensure you have a `.env` file with the following:

```
Telegram__Token=your_bot_token
OpenAI__ApiKey=your_openai_key
Mindee__ApiKey=your_mindee_key
Mindee__PassportEndpoint=https://api.mindee.net/v1/products/.../predict_async
Mindee__VehicleEndpoint=https://api.mindee.net/v1/products/.../predict_async
```

---

## 🏦 Database Schema

Tables:

* Users
* UserStates
* Documents
* ExtractedFields
* Policies
* AuditLogs
* Errors
* Conversations
* PolicyEvents

---

## 🌟 Deployment

You can deploy it to any of the following:

* [x] Render.com (free tier)
* [x] Railway.app
* [x] Azure App Service (Free Tier)

Configure your webhook:

```csharp
await botClient.SetWebhookAsync("https://your-domain.com/api/webhook");
```

---

## 📚 Commands

* `/start` - Begin insurance flow
* `confirm` - Confirm data / price
* `cancel` - Cancel and restart
* `/summary` - Admin summary
* `/logs` - Admin logs
* `/simulate_mindee` - Inject test OCR

---

## 📅 License

MIT

---

## 📄 Author

Shakir Farajullayev
