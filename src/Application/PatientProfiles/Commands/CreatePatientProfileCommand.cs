using Application.Common.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.PatientProfiles.Commands;

public sealed record CreatePatientProfileCommand(
    Guid    PatientId,
    string  FullName,
    string  PhoneNumber,
    DateOnly DateOfBirth,
    string  Gender,
    string  Relation,
    string? Email      = null,
    string? NationalId = null);

public class CreatePatientProfileCommandHandler(
    IPatientProfileRepository profileRepo)
{
    public async Task<PatientProfileDto> HandleAsync(
        CreatePatientProfileCommand cmd, CancellationToken ct = default)
    {
        // PatientId comes from authenticated claims — already verified by auth cookie.
        // No need to re-query the DB for existence; just create the profile directly.

        // First profile is default
        var existing  = await profileRepo.GetByPatientIdAsync(cmd.PatientId, ct);
        var isDefault = existing.Count == 0;

        var profile = PatientProfile.Create(
            cmd.PatientId, cmd.FullName, cmd.PhoneNumber,
            cmd.DateOfBirth, cmd.Gender, cmd.Relation,
            cmd.Email, cmd.NationalId, isDefault);

        await profileRepo.AddAsync(profile, ct);
        await profileRepo.SaveChangesAsync(ct);

        return new PatientProfileDto
        {
            Id          = profile.Id,
            FullName    = profile.FullName,
            PhoneNumber = profile.PhoneNumber,
            DateOfBirth = profile.DateOfBirth,
            Gender      = profile.Gender,
            Relation    = profile.Relation,
            IsDefault   = profile.IsDefault
        };
    }
}
