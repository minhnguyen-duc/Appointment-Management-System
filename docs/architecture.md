# System Architecture — Hospital Digital Redesign 2026

## Clean Architecture Layers

```
Presentation (Blazor)
       ↓ DTOs
  Application (Use Cases)
       ↓ Interfaces
     Domain (Entities)
       ↑ Impl
  Infrastructure (EF Core, APIs)
```

## Capacity Rules
- Max **10 patients/hour** per doctor
- Min **2 hours** advance notice
- Buffer time between slots (configurable)
- Blackout dates managed automatically

## RAG Scope (CRITICAL LIMIT)
The AI pipeline handles **Medical Q&A ONLY**.
It must NOT perform automated booking or diagnostic prescription.

## Third-Party Integrations
| Service | Purpose |
|---|---|
| Twilio | OTP + SMS reminders |
| SendGrid | Email confirmation + QR code |
| VNPAY Sandbox | Deposit / fee payment |
| Google / Outlook / Apple | Calendar two-way sync |
| Azure OpenAI (GPT-4o) | RAG response synthesis |
| Azure AI Search | Vector index / semantic search |
