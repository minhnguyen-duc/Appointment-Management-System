using Application.Common.DTOs;
using Application.Common.Interfaces;

namespace Application.PatientProfiles.Queries;

public sealed record GetPatientProfilesQuery(Guid PatientId);

public class GetPatientProfilesQueryHandler(IPatientProfileRepository profileRepo)
{
    public async Task<List<PatientProfileDto>> HandleAsync(
        GetPatientProfilesQuery query, CancellationToken ct = default)
    {
        var profiles = await profileRepo.GetByPatientIdAsync(query.PatientId, ct);
        return profiles.Select(p => new PatientProfileDto
        {
            Id          = p.Id,
            FullName    = p.FullName,
            PhoneNumber = p.PhoneNumber,
            DateOfBirth = p.DateOfBirth,
            Gender      = p.Gender,
            Relation    = p.Relation,
            IsDefault   = p.IsDefault
        }).ToList();
    }
}
