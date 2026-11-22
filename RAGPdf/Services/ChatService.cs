using System.Text;
using OllamaSharp;

namespace RAGPdf.Services;

public class ChatService
{
    private readonly OllamaApiClient _client;
    private readonly string _endpoint;
    private readonly string _model;

    public ChatService(string endpoint, string model)
    {
        _endpoint = endpoint;
        _model = model;
        _client = new OllamaApiClient(new Uri(endpoint), model);
    }

    public async Task<string> AskAsync(string prompt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        var response = new StringBuilder();

        try
        {
            Console.WriteLine("[Chat] Receiving the generated response...");
            await foreach (var chunk in _client.GenerateAsync(prompt))
            {
                if (chunk?.Response is { } chunkResponse)
                {
                    response.Append(chunkResponse);
                }
            }

            Console.WriteLine("[Chat] Response received from Ollama.");
        }
        catch (HttpRequestException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException(
                $"Ollama could not find the chat model '{_model}' at '{_endpoint}'. " +
                $"Install it with 'ollama pull {_model}' or set Ollama:ChatModel to an installed generative model.",
                exception);
        }

        return response.ToString();
    }
}
