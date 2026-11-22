namespace RAGPdf.Services;

public class RagOrchestrator
{
    private readonly RagIndexer _ragIndexer;
    private readonly ChatService _chatService;

    public RagOrchestrator(RagIndexer ragIndexer, ChatService chatService)
    {
        _ragIndexer = ragIndexer;
        _chatService = chatService;
    }

    public async Task<string> AskQuestionAsync(string question)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);

        var chunks = await _ragIndexer.Search(question);
        Console.WriteLine("[5] Building context from the retrieved chunks...");
        var contentChunks = chunks
            .Select(x => x.Content)
            .Where(content => !string.IsNullOrWhiteSpace(content))
            .ToList();

        if (contentChunks.Count == 0)
        {
            throw new InvalidOperationException(
                "No readable text was found in the indexed PDF. " +
                "Only text-based PDFs are supported; scanned or image-only PDFs require OCR.");
        }

        var context = string.Join(
            Environment.NewLine + Environment.NewLine,
            contentChunks);

        var prompt = PromptBuilder.Create(context, question);
        Console.WriteLine("[6] Prompt built; sending the question and context to the chat model...");

        return await _chatService.AskAsync(prompt);
    }
}
