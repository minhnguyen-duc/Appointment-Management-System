namespace Application.Common.Interfaces;

/// <summary>
/// RAG pipeline — Medical Q&A ONLY. Must NOT perform booking or diagnosis.
/// </summary>
public interface IRagService
{
    Task<string> QueryMedicalFaqAsync(string userQuery, string locale = "en", CancellationToken ct = default);
}
