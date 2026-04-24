using Application.Common.Interfaces;

namespace Presentation.Services;

public class RagClientService(IRagService ragService)
{
    public Task<string> QueryAsync(string question, string locale = "vi")
        => ragService.QueryMedicalFaqAsync(question, locale);
}
