# RAGPdf Technical Document

## 1. Overview

RAGPdf is a .NET 10 console application that answers questions from a text-based PDF using Retrieval-Augmented Generation (RAG).

It uses:

- **PdfPig** to extract PDF text.
- **Ollama** to create embeddings and generate answers.
- **In-memory storage** for document chunks and vectors.
- **Cosine similarity plus keyword matching** to find relevant content.

## 2. Application Flow

```text
CSharp.pdf
	|
	v
Read PDF text
	|
	v
Split text into overlapping chunks
	|
	v
Create an embedding for each chunk
	|
	v
Store chunks in memory
	|
	v
User enters a question
	|
	v
Create question embedding
	|
	v
Rank chunks by similarity and keywords
	|
	v
Build a context-based prompt
	|
	v
Ollama chat model generates the answer
```

## 3. Main Components

| Component | Responsibility |
|---|---|
| `Program.cs` | Loads configuration, indexes the PDF, and reads questions. |
| `PdfService` | Extracts text from the PDF. |
| `ChunkService` | Splits text into 800-character chunks with 120-character overlap. |
| `EmbeddingService` | Creates vectors through Ollama. |
| `RagIndexer` | Indexes chunks and retrieves the top five matches. |
| `SimilarityHelper` | Calculates cosine similarity. |
| `PromptBuilder` | Creates a prompt restricted to retrieved context. |
| `ChatService` | Streams the answer from the Ollama chat model. |
| `RagOrchestrator` | Coordinates retrieval, prompt creation, and response generation. |

## 4. Indexing Code

The PDF is read, split, and embedded before questions are accepted:

```csharp
var text = _pdfService.ReadPdf(path);
var chunks = _chunkService.Split(text);

foreach (var chunk in chunks)
{
	var embedding = await _embeddingService.CreateEmbedding(chunk);
	_chunks.Add(new DocumentChunk
	{
		Content = chunk,
		Embedding = embedding
	});
}
```

## 5. Retrieval Code

For each question, the application creates a query embedding and returns the five highest-scoring chunks:

```csharp
var questionEmbedding =
	await _embeddingService.CreateEmbedding(question);

var matches = _chunks
	.OrderByDescending(chunk => ScoreChunk(
		chunk, questionEmbedding, queryTerms,
		queryTerms.Length, normalizedQuestion))
	.Take(5)
	.ToList();
```

The score combines:

```text
score = cosine similarity
	  + exact phrase boost
	  + keyword coverage × 0.25
```

## 6. Prompt and Answer Generation

Only the retrieved document context is sent to the chat model:

```csharp
var prompt = $"""
You are a helpful assistant.
Answer ONLY using the provided context.
If the answer is not found in the context, say:
"I could not find the answer in the document."

Context:
{context}

Question:
{question}
""";
```

`ChatService` sends this prompt to Ollama and combines the streamed response into one answer.

## 7. Configuration

`RAGPdf/appsettings.json` contains the Ollama settings:

```json
{
  "Ollama": {
	"Endpoint": "http://localhost:11434",
	"EmbeddingModel": "nomic-embed-text",
	"ChatModel": "llama3.2"
  }
}
```

Required preparation:

1. Install and start Ollama.
2. Download the configured models:

```powershell
ollama pull nomic-embed-text
ollama pull llama3.2
```

3. Ensure `RAGPdf/Files/CSharp.pdf` exists.
4. Run the application:

```powershell
dotnet run --project RAGPdf/RAGPdf.csproj
```

## 8. Important Limitations

- Only text-based PDFs are supported. Scanned PDFs require OCR.
- Indexed data is stored in memory and is lost when the application stops.
- The application currently processes one bundled PDF.
- Ollama must be running locally or at the configured endpoint.
- The configured embedding model must return vectors with a consistent dimension.
