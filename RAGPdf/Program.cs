using Microsoft.Extensions.Configuration;
using RAGPdf.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .Build();

var endpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
var embeddingModel = configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text";
var chatModel = configuration["Ollama:ChatModel"] ?? "llama3.2";
var pdfPath = Path.Combine(AppContext.BaseDirectory, "Files", "CSharp.pdf");

if (!File.Exists(pdfPath))
{
    Console.Error.WriteLine($"The PDF file was not found: {pdfPath}");
    return 3;
}

try
{
    var pdfService = new PdfService();
    var chunkService = new ChunkService();
    var embeddingService = new EmbeddingService(endpoint, embeddingModel);
    var ragIndexer = new RagIndexer(pdfService, chunkService, embeddingService);
    var chatService = new ChatService(endpoint, chatModel);
    var ragOrchestrator = new RagOrchestrator(ragIndexer, chatService);

    Console.WriteLine($"Indexing {Path.GetFileName(pdfPath)}...");
    await ragIndexer.IndexPdf(pdfPath);

    Console.WriteLine("PDF indexed. Enter a question, or type 'exit' or 'quit' to terminate.");

    while (true)
    {
        Console.Write("Question: ");
        var question = Console.ReadLine();

        if (question is null ||
            string.Equals(question.Trim(), "exit", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(question.Trim(), "quit", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Terminating.");
            break;
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            Console.WriteLine("Enter a question or type 'exit' to terminate.");
            continue;
        }

        try
        {
            Console.WriteLine("[1] Question received.");
            var answer = await ragOrchestrator.AskQuestionAsync(question);
            Console.WriteLine("[7] Answer completed.");
            Console.WriteLine($"Answer: {answer}");
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Question failed: {exception.Message}");
        }
    }

    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"RAG processing failed: {exception.Message}");
    return 1;
}
