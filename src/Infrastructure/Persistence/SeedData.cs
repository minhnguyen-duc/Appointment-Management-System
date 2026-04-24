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
            Doctor.Create("BS. Nguyễn Văn An", "Tim mạch", "LIC-001"),
            Doctor.Create("BS. Trần Thị Bình", "Nhi khoa", "LIC-002"),
            Doctor.Create("BS. Lê Minh Châu", "Da liễu", "LIC-003"),
            Doctor.Create("BS. Phạm Thị Dung", "Thần kinh", "LIC-004"),
            Doctor.Create("BS. Hoàng Văn Em", "Tai Mũi Họng", "LIC-005"),
        };
        await db.Doctors.AddRangeAsync(doctors);

        var patient = Patient.Create("Nguyễn Văn Test", "0912345678", "test@email.com", new DateOnly(1990, 1, 1));
        await db.Patients.AddAsync(patient);

        await db.SaveChangesAsync();
    }
}
