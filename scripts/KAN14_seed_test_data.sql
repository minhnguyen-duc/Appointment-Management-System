-- ============================================================
-- KAN-14 Test Data Seed Script
-- Database: AppointmentAMS
-- Run AFTER KAN14_migrate.sql
-- Safe to run multiple times
-- ============================================================

USE AppointmentAMS;
GO

-- ============================================================
-- 1. DOCTORS (10 bác sĩ, đa dạng chuyên khoa + học hàm)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Doctors WHERE LicenseNumber = 'LIC-001')
INSERT INTO Doctors (Id, FullName, AcademicTitle, Specialization, LicenseNumber, ConsultationFee, ImageUrl, Bio, IsActive)
VALUES
    ('A1000001-0000-0000-0000-000000000001', 'Nguyễn Văn An',   'TS.BS',      'Tim mạch',    'LIC-001', 350000, NULL,
     'Chuyên gia tim mạch can thiệp với hơn 15 năm kinh nghiệm. Từng tu nghiệp tại Pháp và Hoa Kỳ.', 1),
    ('A1000001-0000-0000-0000-000000000002', 'Trần Thị Bình',   'PGS.TS.BS',  'Nhi khoa',    'LIC-002', 280000, NULL,
     'Phó Giáo sư chuyên về nhi khoa sơ sinh. Kinh nghiệm 20 năm điều trị bệnh lý trẻ em.', 1),
    ('A1000001-0000-0000-0000-000000000003', 'Lê Minh Châu',    'BS.CKI',     'Da liễu',     'LIC-003', 250000, NULL,
     'Bác sĩ chuyên khoa I da liễu. Điều trị các bệnh da mãn tính, thẩm mỹ da.', 1),
    ('A1000001-0000-0000-0000-000000000004', 'Phạm Thị Dung',   'TS.BS',      'Thần kinh',   'LIC-004', 400000, NULL,
     'Tiến sĩ Thần kinh học, chuyên về đột quỵ não và Parkinson. 18 năm kinh nghiệm lâm sàng.', 1),
    ('A1000001-0000-0000-0000-000000000005', 'Hoàng Văn Em',    'BS.CKII',    'Tai Mũi Họng','LIC-005', 220000, NULL,
     'Bác sĩ chuyên khoa II Tai Mũi Họng. Phẫu thuật nội soi mũi xoang.', 1),
    ('A1000001-0000-0000-0000-000000000006', 'Vũ Thị Phương',   'TS.BS',      'Tiêu hóa',    'LIC-006', 320000, NULL,
     'Chuyên gia tiêu hóa – nội soi can thiệp. Điều trị viêm loét dạ dày, viêm đại tràng.', 1),
    ('A1000001-0000-0000-0000-000000000007', 'Đặng Minh Quân',  'BS',         'Mắt',         'LIC-007', 200000, NULL,
     'Bác sĩ nhãn khoa. Phẫu thuật khúc xạ laser, điều trị mắt hột, đục thủy tinh thể.', 1),
    ('A1000001-0000-0000-0000-000000000008', 'Ngô Thị Hoa',     'PGS.TS.BS',  'Nội tổng quát','LIC-008',300000, NULL,
     'Phó Giáo sư nội khoa. Chẩn đoán và điều trị bệnh nội khoa phức tạp.', 1),
    ('A1000001-0000-0000-0000-000000000009', 'Bùi Văn Sơn',     'BS.CKI',     'Cơ xương khớp','LIC-009',260000, NULL,
     'Chuyên khoa I Cơ xương khớp. Điều trị viêm khớp, thoái hóa khớp, loãng xương.', 1),
    ('A1000001-0000-0000-0000-000000000010', 'Cao Thị Lan',     'TS.BS',      'Nhi khoa',    'LIC-010', 290000, NULL,
     'Tiến sĩ Nhi khoa, chuyên về miễn dịch – dị ứng trẻ em. 12 năm kinh nghiệm.', 1);
GO

-- ============================================================
-- 2. PATIENTS (3 bệnh nhân test)
-- ============================================================
-- Patient 1: có password (dùng để test login)
-- Password hash của "Test@1234" với salt "ams2026-salt"
IF NOT EXISTS (SELECT 1 FROM Patients WHERE PhoneNumber = '0912345678')
INSERT INTO Patients (Id, FullName, PhoneNumber, Email, DateOfBirth, NationalId, CreatedAt, PasswordHash, FailedPasswordAttempts)
VALUES (
    'B2000001-0000-0000-0000-000000000001',
    'Nguyễn Mai',
    '0912345678',
    'nguyenmai@test.com',
    '1990-05-15',
    '079090001234',
    GETUTCDATE(),
    -- SHA256("Test@1234" + "ams2026-salt") — pre-computed
    UPPER(CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', N'Test@1234ams2026-salt'), 2)),
    0
);

IF NOT EXISTS (SELECT 1 FROM Patients WHERE PhoneNumber = '0987654321')
INSERT INTO Patients (Id, FullName, PhoneNumber, Email, DateOfBirth, NationalId, CreatedAt, PasswordHash, FailedPasswordAttempts)
VALUES (
    'B2000001-0000-0000-0000-000000000002',
    'Trần Văn Bình',
    '0987654321',
    'tranbinh@test.com',
    '1985-08-20',
    '079085005678',
    GETUTCDATE(),
    UPPER(CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', N'Test@1234ams2026-salt'), 2)),
    0
);

-- Patient 3: chưa có password (test OTP flow)
IF NOT EXISTS (SELECT 1 FROM Patients WHERE PhoneNumber = '0909123456')
INSERT INTO Patients (Id, FullName, PhoneNumber, Email, DateOfBirth, NationalId, CreatedAt, FailedPasswordAttempts)
VALUES (
    'B2000001-0000-0000-0000-000000000003',
    'Lê Thị Cúc',
    '0909123456',
    'lecuc@test.com',
    '1995-12-01',
    NULL,
    GETUTCDATE(),
    0
);
GO

-- ============================================================
-- 3. PATIENT PROFILES (2 profiles cho Patient 1)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM PatientProfiles WHERE PatientId = 'B2000001-0000-0000-0000-000000000001')
INSERT INTO PatientProfiles (Id, PatientId, FullName, PhoneNumber, Email, DateOfBirth, Gender, Relation, NationalId, IsDefault, CreatedAt)
VALUES
    -- Profile mặc định: bản thân
    ('C3000001-0000-0000-0000-000000000001',
     'B2000001-0000-0000-0000-000000000001',
     'Nguyễn Mai', '0912345678', 'nguyenmai@test.com',
     '1990-05-15', 'Nữ', 'Bản thân', '079090001234', 1, GETUTCDATE()),
    -- Profile phụ: mẹ
    ('C3000001-0000-0000-0000-000000000002',
     'B2000001-0000-0000-0000-000000000001',
     'Trần Thị Hoa', '0912345678', NULL,
     '1965-03-08', 'Nữ', 'Mẹ', NULL, 0, GETUTCDATE());
GO

-- ============================================================
-- 4. APPOINTMENTS (lịch hẹn mẫu)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Appointments WHERE PatientId = 'B2000001-0000-0000-0000-000000000001')
INSERT INTO Appointments (
    Id, PatientId, DoctorId, ProfileId,
    ScheduledAt, DurationMinutes, Status,
    ConsultationFee, PaymentStatus, PaymentReference,
    SequenceNumber, RoomNumber, BarcodeData, ETicketSent,
    Notes, CreatedAt
)
VALUES
    -- Lịch đã xác nhận + thanh toán + có e-ticket
    ('D4000001-0000-0000-0000-000000000001',
     'B2000001-0000-0000-0000-000000000001',
     'A1000001-0000-0000-0000-000000000001',  -- BS. Nguyễn Văn An (Tim mạch)
     'C3000001-0000-0000-0000-000000000001',  -- Profile: Nguyễn Mai
     DATEADD(DAY, 3, GETDATE()),              -- 3 ngày tới
     60, 2,                                   -- Status=Confirmed
     350000, 'Paid', 'VNPAY-TEST-001',
     5, 'P.201', 'D4000001D40000', 1,
     'Tái khám tim mạch định kỳ', GETUTCDATE()),

    -- Lịch đang chờ xác nhận (chưa thanh toán)
    ('D4000001-0000-0000-0000-000000000002',
     'B2000001-0000-0000-0000-000000000001',
     'A1000001-0000-0000-0000-000000000004',  -- BS. Phạm Thị Dung (Thần kinh)
     'C3000001-0000-0000-0000-000000000001',
     DATEADD(DAY, 7, GETDATE()),              -- 7 ngày tới
     60, 1,                                   -- Status=Pending
     400000, 'Unpaid', NULL,
     NULL, NULL, NULL, 0,
     'Đau đầu mãn tính', GETUTCDATE()),

    -- Lịch đã hoàn thành (lịch sử)
    ('D4000001-0000-0000-0000-000000000003',
     'B2000001-0000-0000-0000-000000000001',
     'A1000001-0000-0000-0000-000000000002',  -- BS. Trần Thị Bình (Nhi khoa)
     'C3000001-0000-0000-0000-000000000002',  -- Profile: Mẹ
     DATEADD(DAY, -14, GETDATE()),            -- 2 tuần trước
     60, 4,                                   -- Status=Completed
     280000, 'Paid', 'VNPAY-TEST-002',
     12, 'P.105', 'D4000001D40000', 1,
     NULL, GETUTCDATE());
GO

-- ============================================================
-- VERIFY
-- ============================================================
SELECT 'Doctors'        AS [Table], COUNT(*) AS [Rows] FROM Doctors
UNION ALL
SELECT 'Patients',       COUNT(*) FROM Patients
UNION ALL
SELECT 'PatientProfiles',COUNT(*) FROM PatientProfiles
UNION ALL
SELECT 'Appointments',   COUNT(*) FROM Appointments;

PRINT '';
PRINT '=== Test credentials ===';
PRINT 'Phone: 0912345678 | Password: Test@1234  (Nguyễn Mai)';
PRINT 'Phone: 0987654321 | Password: Test@1234  (Trần Văn Bình)';
PRINT 'Phone: 0909123456 | No password — OTP flow (Lê Thị Cúc)';
GO
