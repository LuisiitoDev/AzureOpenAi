using Azure.AI.OpenAI;

namespace CostaRicaBeaches.Api.Interfaces;

public interface IAzureAISearchService
{
    ChatCompletionsOptions GetChatCompletions(string userMessage, string deploymentName);
}
