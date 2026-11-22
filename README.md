# RAGPdf

A simple .NET 10 console application that answers questions from a PDF using Retrieval-Augmented Generation (RAG).

## How It Works

```text
PDF
 ↓
Extract text
 ↓
Split into overlapping chunks
 ↓
Create embeddings with Ollama
 ↓
Retrieve the most relevant chunks
 ↓
Build a context-based prompt
 ↓
Generate an answer with Ollama
```

## Requirements

- .NET 10 SDK
- Ollama
- A text-based PDF
- Ollama models:
  - `nomic-embed-text`
  - `llama3.2`

Install the models:

```powershell
ollama pull nomic-embed-text
ollama pull llama3.2
```

## Configuration

The Ollama configuration is in `RAGPdf/appsettings.json`:

```json
{
  "Ollama": {
	"Endpoint": "http://localhost:11434",
	"EmbeddingModel": "nomic-embed-text",
	"ChatModel": "llama3.2"
  }
}
```

## Run the Application

Ensure the PDF exists at:

```text
RAGPdf/Files/CSharp.pdf
```

Start Ollama, then run:

```powershell
dotnet run --project RAGPdf/RAGPdf.csproj
```

Enter a question at the prompt. Type `exit` or `quit` to stop the application.

## Project Structure

| Path | Description |
|---|---|
| `RAGPdf/Program.cs` | Application startup and console input loop |
| `RAGPdf/Services/PdfService.cs` | Extracts text from the PDF |
| `RAGPdf/Services/ChunkService.cs` | Splits text into chunks |
| `RAGPdf/Services/EmbeddingService.cs` | Creates embeddings with Ollama |
| `RAGPdf/Services/RagIndexer.cs` | Indexes and searches document chunks |
| `RAGPdf/Services/RagOrchestrator.cs` | Coordinates retrieval and answer generation |
| `RAGPdf/Services/ChatService.cs` | Generates answers with Ollama |
| `RAGPdf/Services/PromptBuilder.cs` | Creates the RAG prompt |
| `RAGPdf/Models/DocumentChunk.cs` | Stores chunk text and embeddings |

## Limitations

- Only text-based PDFs are supported. Scanned PDFs require OCR.
- Document vectors are stored in memory and are lost when the application stops.
- The application currently processes one bundled PDF.
- Ollama must be running at the configured endpoint.

## Documentation

See [RAGPdf-Technical-Document.md](RAGPdf-Technical-Document.md) for a more detailed technical overview.

## License

Add a license before publishing this project publicly.
