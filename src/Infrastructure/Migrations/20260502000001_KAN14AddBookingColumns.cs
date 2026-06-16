using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class KAN14AddBookingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Doctor: KAN-14 AC1 new columns ──────────────────────────────
            migrationBuilder.AddColumn<string>(
                name: "AcademicTitle",
                table: "Doctors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "BS.");

            migrationBuilder.AddColumn<decimal>(
                name: "ConsultationFee",
                table: "Doctors",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Doctors",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Doctors",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            // ── Appointment: KAN-14 payment + e-ticket columns ──────────────
            migrationBuilder.AddColumn<Guid>(
                name: "ProfileId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ConsultationFee",
                table: "Appointments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Appointments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Unpaid");

            migrationBuilder.AddColumn<int>(
                name: "SequenceNumber",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomNumber",
                table: "Appointments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BarcodeData",
                table: "Appointments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ETicketSent",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Migrate Status column from int to nvarchar (was int in initial migration)
            // Only add if it's not already nvarchar
            // (AppointmentConfiguration now stores Status as string)

            // ── PatientProfile: new table (KAN-14 AC2) ──────────────────────
            migrationBuilder.CreateTable(
                name: "PatientProfiles",
                columns: table => new
                {
                    Id          = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId   = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName    = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)",  maxLength: 20,  nullable: false),
                    Email       = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender      = table.Column<string>(type: "nvarchar(10)",  maxLength: 10,  nullable: false),
                    Relation    = table.Column<string>(type: "nvarchar(50)",  maxLength: 50,  nullable: false),
                    NationalId  = table.Column<string>(type: "nvarchar(20)",  maxLength: 20,  nullable: true),
                    IsDefault   = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt   = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientProfiles_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientProfiles_PatientId",
                table: "PatientProfiles",
                column: "PatientId");

            // Update existing Doctors seed data with default values
            migrationBuilder.Sql(@"
                UPDATE Doctors
                SET AcademicTitle   = 'BS.',
                    ConsultationFee = 300000
                WHERE AcademicTitle IS NULL OR AcademicTitle = ''
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PatientProfiles");

            migrationBuilder.DropColumn(name: "AcademicTitle",   table: "Doctors");
            migrationBuilder.DropColumn(name: "ConsultationFee", table: "Doctors");
            migrationBuilder.DropColumn(name: "ImageUrl",        table: "Doctors");
            migrationBuilder.DropColumn(name: "Bio",             table: "Doctors");

            migrationBuilder.DropColumn(name: "ProfileId",       table: "Appointments");
            migrationBuilder.DropColumn(name: "ConsultationFee", table: "Appointments");
            migrationBuilder.DropColumn(name: "PaymentStatus",   table: "Appointments");
            migrationBuilder.DropColumn(name: "SequenceNumber",  table: "Appointments");
            migrationBuilder.DropColumn(name: "RoomNumber",      table: "Appointments");
            migrationBuilder.DropColumn(name: "BarcodeData",     table: "Appointments");
            migrationBuilder.DropColumn(name: "ETicketSent",     table: "Appointments");
        }
    }
}
