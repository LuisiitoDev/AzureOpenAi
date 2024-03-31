using Azure;
using Azure.AI.OpenAI;
using CostaRicaBeaches.Api.Configurations;
using CostaRicaBeaches.Api.Interfaces;
using Microsoft.Extensions.Options;

namespace CostaRicaBeaches.Api.Services;

public class AzureOpenAIService(IOptions<AzureOpenAIOptions> options, IAzureAISearchService azureAISearch) : IAzureOpenAIService
{
    public async Task<string> SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        var client = GetOpenAIClient();
        var response = await client.GetChatCompletionsAsync(azureAISearch.GetChatCompletions(message, options.Value.Deployment!), cancellationToken);
        var aiMessage = response.Value.Choices[0].Message;
        return aiMessage.Content;
    }

    private OpenAIClient GetOpenAIClient()
    {
        return new OpenAIClient(
            new Uri(options.Value.Endpoint!),
            new AzureKeyCredential(options.Value.ApiKey!));
    }
}
