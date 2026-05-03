using Application.Common.DTOs;
using Application.Common.Interfaces;

namespace Application.Booking.Queries;

public sealed record GetDoctorCatalogQuery(string? Specialization = null, string? Keyword = null);

public class GetDoctorCatalogQueryHandler(IDoctorQueryService doctorQuery)
{
    public async Task<List<DoctorCatalogDto>> HandleAsync(
        GetDoctorCatalogQuery query, CancellationToken ct = default)
    {
        var doctors = await doctorQuery.GetCatalogAsync(
            query.Specialization, query.Keyword, ct);
        return doctors;
    }
}
