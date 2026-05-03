-- KAN-14 Migration Script — Run directly on AppointmentAMS database
-- Safe to run multiple times (IF NOT EXISTS checks on every column/table)

USE AppointmentAMS;
GO

-- ── Doctors ─────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Doctors') AND name='AcademicTitle')
    ALTER TABLE Doctors ADD AcademicTitle NVARCHAR(50) NOT NULL DEFAULT 'BS.';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Doctors') AND name='ConsultationFee')
    ALTER TABLE Doctors ADD ConsultationFee DECIMAL(18,2) NOT NULL DEFAULT 300000;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Doctors') AND name='ImageUrl')
    ALTER TABLE Doctors ADD ImageUrl NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Doctors') AND name='Bio')
    ALTER TABLE Doctors ADD Bio NVARCHAR(1000) NULL;

-- ── Appointments ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Appointments') AND name='ProfileId')
    ALTER TABLE Appointments ADD ProfileId UNIQUEIDENTIFIER NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Appointments') AND name='ConsultationFee')
    ALTER TABLE Appointments ADD ConsultationFee DECIMAL(18,2) NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Appointments') AND name='PaymentStatus')
    ALTER TABLE Appointments ADD PaymentStatus NVARCHAR(20) NOT NULL DEFAULT 'Unpaid';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Appointments') AND name='SequenceNumber')
    ALTER TABLE Appointments ADD SequenceNumber INT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Appointments') AND name='RoomNumber')
    ALTER TABLE Appointments ADD RoomNumber NVARCHAR(20) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Appointments') AND name='BarcodeData')
    ALTER TABLE Appointments ADD BarcodeData NVARCHAR(50) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Appointments') AND name='ETicketSent')
    ALTER TABLE Appointments ADD ETicketSent BIT NOT NULL DEFAULT 0;

-- ── PatientProfiles (new table) ──────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='PatientProfiles')
BEGIN
    CREATE TABLE PatientProfiles (
        Id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        PatientId   UNIQUEIDENTIFIER NOT NULL,
        FullName    NVARCHAR(150)    NOT NULL,
        PhoneNumber NVARCHAR(20)     NOT NULL,
        Email       NVARCHAR(200)    NULL,
        DateOfBirth DATE             NOT NULL,
        Gender      NVARCHAR(10)     NOT NULL,
        Relation    NVARCHAR(50)     NOT NULL,
        NationalId  NVARCHAR(20)     NULL,
        IsDefault   BIT              NOT NULL DEFAULT 0,
        CreatedAt   DATETIME2        NOT NULL,
        CONSTRAINT FK_PatientProfiles_Patients
            FOREIGN KEY (PatientId) REFERENCES Patients(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_PatientProfiles_PatientId ON PatientProfiles(PatientId);
END

-- ── Register in __EFMigrationsHistory so EF won't try again ─────────────────
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE MigrationId='20260502000001_KAN14AddBookingColumns')
    INSERT INTO [__EFMigrationsHistory] (MigrationId, ProductVersion)
    VALUES ('20260502000001_KAN14AddBookingColumns', '9.0.4');

PRINT 'Done.';
GO
