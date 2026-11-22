using System.Text;
using UglyToad.PdfPig;

namespace RAGPdf.Services;

public class PdfService
{
    public string ReadPdf(string path)
    {
        using var pdf = PdfDocument.Open(path);
        var sb = new StringBuilder();

        foreach (var page in pdf.GetPages())
        {
            sb.AppendLine(page.Text);
        }

        var text = sb.ToString();

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException(
                $"No readable text was found in '{Path.GetFileName(path)}'. " +
                "Only text-based PDFs are supported; scanned or image-only PDFs require OCR.");
        }

        return text;
    }
}
