# Appointment Management System
## Hospital Digital Redesign 2026

A cloud-native, AI-assisted appointment management platform built with Clean Architecture on .NET 10.

## Tech Stack
| Layer | Technology |
|---|---|
| Framework | .NET 10 (LTS) |
| UI | Blazor Web App (Server + WebAssembly) |
| Database | MSSQL + Entity Framework Core |
| AI / Search | Azure OpenAI (GPT-4o) + Azure AI Search |
| Orchestration | Semantic Kernel |
| SMS | Twilio |
| Email | SendGrid |
| Payment | VNPAY Sandbox |
| Calendar Sync | Google / Outlook / Apple (iCal) |

## Architecture
```
AppointmentManagementSystem/
├── src/
│   ├── Domain/            # Enterprise Business Rules (zero external dependencies)
│   ├── Application/       # Use Cases & Interfaces
│   ├── Infrastructure/    # DB, Identity, External APIs, AI
│   └── Presentation/      # Blazor Web App
└── tests/
    ├── Domain.UnitTests/
    ├── Application.UnitTests/
    ├── Infrastructure.IntegrationTests/
    └── Presentation.E2ETests/
```

## Getting Started
```bash
dotnet restore
dotnet build
dotnet run --project src/Presentation
```

## Scheduling Rules
- Max **10 patients/hour** per doctor/clinic
- Minimum notice: **2 hours** before appointment
- Configurable buffer between slots for sanitization
- Automated blackout date management
