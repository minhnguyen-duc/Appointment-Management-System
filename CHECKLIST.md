# 🚀 Initial Run Checklist

## Prerequisites
- [x] .NET 10 SDK — https://dotnet.microsoft.com/download
- [x] SQL Server (LocalDB đủ dùng, tích hợp sẵn trong Visual Studio)  
      Hoặc: `winget install Microsoft.SQLServer.2022.Express`
- [x] Visual Studio 2022 v17.12+ / Rider / VS Code + C# extension

---

## Bước 1 — Clone & Restore
```bash
git clone https://github.com/minhnguyen-duc/Appointment-Management-System.git
cd Appointment-Management-System
dotnet restore
```

## Bước 2 — Cấu hình Connection String
Mở file `src/Presentation/appsettings.Development.json`.

Nếu dùng **SQL Server LocalDB** (mặc định, không cần cài thêm):
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AppointmentManagementSystem_Dev;Trusted_Connection=True;"
```

Nếu dùng **SQL Server Express**:
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=AppointmentManagementSystem_Dev;Trusted_Connection=True;"
```

## Bước 3 — Tạo Migration & Database
```bash
cd src/Infrastructure

dotnet ef migrations add InitialCreate \
  --startup-project ../Presentation \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --startup-project ../Presentation
```

> ⚡ Nếu bạn chạy `dotnet run` ở Bước 4, app sẽ **tự động** chạy migrate + seed data khi khởi động (đã cấu hình trong Program.cs).

## Bước 4 — Chạy ứng dụng
```bash
cd src/Presentation
dotnet run
```
Mở trình duyệt: **https://localhost:5001**

---

## Bước 5 — Cấu hình các dịch vụ ngoài (tuỳ chọn cho dev)

| Dịch vụ | Bắt buộc? | Ghi chú |
|---|---|---|
| SQL Server | ✅ Bắt buộc | LocalDB đủ dùng |
| Twilio (OTP) | ❌ Tuỳ chọn | Không có → OTP sẽ log ra console |
| SendGrid (Email) | ❌ Tuỳ chọn | Không có → Email không gửi nhưng app vẫn chạy |
| VNPAY | ❌ Tuỳ chọn | Dùng sandbox, không cần thật |
| Azure OpenAI + AI Search | ❌ Tuỳ chọn | Không có → trang Medical FAQ trả về placeholder |

### Dev workaround — OTP log ra console
Mở `src/Infrastructure/ExternalServices/Twilio/TwilioSmsService.cs`, sửa `SendOtpAsync`:
```csharp
public async Task SendOtpAsync(string phoneNumber, string otp, CancellationToken ct = default)
{
    Console.WriteLine($"[DEV OTP] {phoneNumber} → {otp}"); // xem trong terminal
    await Task.CompletedTask;
}
```

---

## Cấu trúc Database sẽ được tạo
```
Tables:
  Patients     — hồ sơ bệnh nhân
  Doctors      — thông tin bác sĩ  
  Appointments — lịch hẹn (FK → Patients, Doctors)

Seed data (tự động):
  5 bác sĩ mẫu (Tim mạch, Nhi, Da liễu, Thần kinh, TMH)
  1 bệnh nhân test: SĐT 0912345678
```

---

## Lỗi thường gặp

| Lỗi | Nguyên nhân | Giải pháp |
|---|---|---|
| `Cannot open database` | LocalDB chưa khởi động | Chạy `sqllocaldb start MSSQLLocalDB` |
| `No migrations found` | Chưa chạy `ef migrations add` | Chạy Bước 3 |
| `Port 5001 in use` | Cổng bị chiếm | Thêm `--urls http://localhost:5002` vào `dotnet run` |
| Build error: namespace | Thiếu `using` | Kiểm tra `_Imports.razor` |

---

## Chạy Unit Tests
```bash
dotnet test tests/Domain.UnitTests
dotnet test tests/Application.UnitTests
```
