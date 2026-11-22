using System.Text;

namespace RAGPdf.Services;

public class ChunkService
{
    public List<string> Split(string text, int chunkSize = 800, int overlap = 120)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (chunkSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be greater than zero.");
        }

        if (overlap < 0 || overlap >= chunkSize)
        {
            throw new ArgumentOutOfRangeException(nameof(overlap), "Overlap must be non-negative and smaller than the chunk size.");
        }

        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<string>();
        var start = 0;

        while (start < words.Length)
        {
            var builder = new StringBuilder();
            var end = start;

            while (end < words.Length &&
                   (builder.Length == 0 || builder.Length + words[end].Length + 1 <= chunkSize))
            {
                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(words[end]);
                end++;
            }

            chunks.Add(builder.ToString());

            if (end == words.Length)
            {
                break;
            }

            var nextStart = end;
            var overlapLength = 0;

            while (nextStart > start && overlapLength < overlap)
            {
                nextStart--;
                overlapLength += words[nextStart].Length + 1;
            }

            start = nextStart;
        }

        return chunks;
    }
}
