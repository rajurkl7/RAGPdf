namespace RAGPdf.Services;

public static class PromptBuilder
{
    public static string Create(string context, string question)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(question);

        return $"""
            You are a helpful assistant.

            Answer ONLY using the provided context.

            If the answer is not found in the context, say:
            "I could not find the answer in the document."

            Context:
            {context}

            Question:
            {question}
            """;
    }
}
