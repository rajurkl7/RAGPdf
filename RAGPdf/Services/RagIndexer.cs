using RAGPdf.Models;

namespace RAGPdf.Services;

public class RagIndexer
{
    private readonly PdfService _pdfService;
    private readonly ChunkService _chunkService;
    private readonly EmbeddingService _embeddingService;

    private readonly List<DocumentChunk> _chunks = [];

    public RagIndexer(
        PdfService pdfService,
        ChunkService chunkService,
        EmbeddingService embeddingService)
    {
        _pdfService = pdfService;
        _chunkService = chunkService;
        _embeddingService = embeddingService;
    }

    public async Task IndexPdf(string path)
    {
        var text = _pdfService.ReadPdf(path);

        var chunks = _chunkService
            .Split(text)
            .Where(chunk => !string.IsNullOrWhiteSpace(chunk))
            .ToList();

        if (chunks.Count == 0)
        {
            throw new InvalidOperationException(
                $"No readable text was found in '{Path.GetFileName(path)}'. " +
                "Only text-based PDFs are supported; scanned or image-only PDFs require OCR.");
        }

        Console.WriteLine($"[Indexing] Generating embeddings for {chunks.Count} chunks...");

        for (var index = 0; index < chunks.Count; index++)
        {
            var chunk = chunks[index];
            var embedding = await _embeddingService.CreateEmbedding(chunk);

            _chunks.Add(
                new DocumentChunk
                {
                    Content = chunk,
                    Embedding = embedding
                });

            Console.WriteLine($"[Indexing] Chunk {index + 1}/{chunks.Count} embedded.");
        }
    }

    public async Task<List<DocumentChunk>> Search(string question)
    {
        Console.WriteLine("[2] Generating an embedding for the question...");
        var questionEmbedding = await _embeddingService.CreateEmbedding(question);
        Console.WriteLine("[3] Comparing the question with indexed document chunks...");

        var queryTerms = GetTerms(question);
        var queryTermCount = queryTerms.Length;
        var normalizedQuestion = string.Join(' ', queryTerms);

        var matches = _chunks
            .OrderByDescending(x => ScoreChunk(
                x,
                questionEmbedding,
                queryTerms,
                queryTermCount,
                normalizedQuestion))
            .Take(5)
            .ToList();

        Console.WriteLine($"[4] Retrieved {matches.Count} relevant chunks.");
        return matches;
    }

    private static double ScoreChunk(
        DocumentChunk chunk,
        float[] questionEmbedding,
        string[] queryTerms,
        int queryTermCount,
        string normalizedQuestion)
    {
        var similarity = SimilarityHelper.CosineSimilarity(questionEmbedding, chunk.Embedding);
        var contentTerms = GetTerms(chunk.Content);
        var normalizedContent = string.Join(' ', contentTerms);
        var matchingTerms = 0;

        foreach (var term in queryTerms)
        {
            if (Array.IndexOf(contentTerms, term) >= 0)
            {
                matchingTerms++;
            }
        }

        var termCoverage = queryTermCount == 0
            ? 0
            : (double)matchingTerms / queryTermCount;
        var exactPhraseBoost = normalizedQuestion.Length > 0 &&
                               normalizedContent.Contains(normalizedQuestion, StringComparison.Ordinal)
            ? 1
            : 0;

        return similarity + exactPhraseBoost + termCoverage * 0.25;
    }

    private static string[] GetTerms(string text)
    {
        var normalized = string.Concat(
            text.Select(character => char.IsLetterOrDigit(character)
                ? char.ToLowerInvariant(character)
                : ' '));

        return normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToArray();
    }
}
