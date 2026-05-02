using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Doctors.AnyAsync()) return;

        var doctors = new[]
        {
            Doctor.Create("BS. Nguyễn Văn An", "TS.BS", "Tim mạch", "LIC-001", 300_000m),
            Doctor.Create("BS. Trần Thị Bình", "TS.BS", "Nhi khoa", "LIC-002", 300_000m),
            Doctor.Create("BS. Lê Minh Châu", "TS.BS", "Da liễu", "LIC-003", 300_000m),
            Doctor.Create("BS. Phạm Thị Dung", "TS.BS", "Thần kinh", "LIC-004", 300_000m),
            Doctor.Create("BS. Hoàng Văn Em", "TS.BS", "Tai Mũi Họng", "LIC-005", 300_000m),
        };
        await db.Doctors.AddRangeAsync(doctors);

        var patient = Patient.Create("Nguyễn Văn Test", "0912345678", "test@email.com", new DateOnly(1990, 1, 1));
        await db.Patients.AddAsync(patient);

        await db.SaveChangesAsync();
    }
}
