namespace CostaRicaBeaches.Api.Interfaces;

public interface IAzureOpenAIService
{
    Task<string> SendMessageAsync(string message, CancellationToken cancellationToken);
}
