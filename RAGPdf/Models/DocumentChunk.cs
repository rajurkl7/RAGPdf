namespace RAGPdf.Models;

public class DocumentChunk
{
    public string Content { get; set; } = string.Empty;

    public float[] Embedding { get; set; } = [];
}
