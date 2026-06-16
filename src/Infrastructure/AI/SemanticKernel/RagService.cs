using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.AI.SemanticKernel;

/// <summary>
/// Medical Q&A via Azure OpenAI (GPT-4o) + Azure AI Search.
/// Scope: Information retrieval ONLY — no booking, no diagnosis (spec §4.1).
/// </summary>
public class RagService(IConfiguration config, ILogger<RagService> logger) : IRagService
{
    // Read Azure settings from config (spec §4.2)
    private readonly string _openAiEndpoint = config["Azure:OpenAI:Endpoint"] ?? "";
    private readonly string _searchEndpoint  = config["Azure:Search:Endpoint"] ?? "";

    public async Task<string> QueryMedicalFaqAsync(string userQuery, string locale = "en", CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_openAiEndpoint) || string.IsNullOrEmpty(_searchEndpoint))
        {
            logger.LogWarning("[DEV] Azure OpenAI/Search not configured. Returning placeholder.");
            return "Dịch vụ RAG chưa được cấu hình. Vui lòng liên hệ quản trị viên.";
        }

        // TODO: implement full RAG pipeline
        // 1. Vectorize query → Azure AI Search (hybrid semantic search)
        // 2. Retrieve top-k medical FAQ chunks
        // 3. Send chunks + query to GPT-4o via Semantic Kernel
        // 4. Return grounded natural language response
        logger.LogInformation("RAG query: {Query} [locale={Locale}]", userQuery, locale);
        await Task.CompletedTask;
        return "RAG pipeline placeholder response.";
    }
}
