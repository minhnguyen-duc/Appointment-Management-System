using Application.Common.Interfaces;
using Domain.Entities;
using Application.Common.DTOs;

namespace Infrastructure.Persistence.Repositories;

public class PatientCommandService(AppDbContext db) : IPatientCommandService
{
    public async Task CreateAsync(PatientFormModel model, CancellationToken ct = default)
    {
        var patient = Patient.Create(model.FullName, model.PhoneNumber, model.Email, model.DateOfBirth);
        await db.Patients.AddAsync(patient, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Guid id, PatientFormModel model, CancellationToken ct = default)
    {
        // Domain entity update — extend Patient with Update() method as needed
        await db.SaveChangesAsync(ct);
    }
}
