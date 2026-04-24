using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.AI.SemanticKernel;

/// <summary>
/// Medical Q&A via Azure OpenAI (GPT-4o) + Azure AI Search.
/// Scope: Information retrieval ONLY — no booking, no diagnosis.
/// </summary>
public class RagService(IConfiguration config) : IRagService
{
    public async Task<string> QueryMedicalFaqAsync(string userQuery, string locale = "en", CancellationToken ct = default)
    {
        // 1. Vectorize query with Azure AI Search
        // 2. Retrieve top-k document chunks
        // 3. Send to GPT-4o via Semantic Kernel for grounded response
        await Task.CompletedTask;
        return "RAG pipeline placeholder response.";
    }
}
