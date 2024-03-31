using Azure.AI.OpenAI;
using CostaRicaBeaches.Api.Configurations;
using CostaRicaBeaches.Api.Interfaces;
using Microsoft.Extensions.Options;

namespace CostaRicaBeaches.Api.Services;

public class AzureAISearchService(IOptions<AzureAISearchOptions> azureAISeach) : IAzureAISearchService
{
    public ChatCompletionsOptions GetChatCompletions(string userMessage, string deploymentName) => new()
    {
        Messages =
        {
            new ChatRequestUserMessage(userMessage)
        },
        AzureExtensionsOptions = new()
        {
            Extensions =
            {
                new AzureSearchChatExtensionConfiguration()
                {
                    SearchEndpoint = new Uri(azureAISeach.Value.Endpoint!),
                    IndexName = azureAISeach.Value.Index!,
                    Authentication = new OnYourDataApiKeyAuthenticationOptions(azureAISeach.Value.ApiKey),
                }
            }
        },
        DeploymentName = deploymentName,
        MaxTokens = 800,
        Temperature = 0,
    };
}
