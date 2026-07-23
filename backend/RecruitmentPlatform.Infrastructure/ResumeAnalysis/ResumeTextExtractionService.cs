using System.Text;
using DocumentFormat.OpenXml.Packaging;
using RecruitmentPlatform.Application.Interfaces;
using UglyToad.PdfPig;

namespace RecruitmentPlatform.Infrastructure.ResumeAnalysis;

public sealed class ResumeTextExtractionService
    : IResumeTextExtractionService
{
    private readonly IFileStorageService
        _fileStorageService;

    public ResumeTextExtractionService(
        IFileStorageService fileStorageService)
    {
        _fileStorageService =
            fileStorageService;
    }

    public async Task<string> ExtractTextAsync(
        string storagePath,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        await using var source =
            await _fileStorageService
                .OpenReadAsync(
                    storagePath,
                    cancellationToken);

        if (source is null)
        {
            throw new FileNotFoundException(
                "The stored resume file could not be found.",
                storagePath);
        }

        await using var memory =
            new MemoryStream();

        await source.CopyToAsync(
            memory,
            cancellationToken);

        memory.Position = 0;

        var extension =
            Path.GetExtension(
                originalFileName)
                .ToLowerInvariant();

        var text = extension switch
        {
            ".pdf" =>
                ExtractPdf(
                    memory,
                    cancellationToken),

            ".docx" =>
                ExtractDocx(memory),

            _ => throw new NotSupportedException(
                "Only PDF and DOCX resume analysis is supported.")
        };

        text = NormalizeText(text);

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException(
                "No readable text was found in the resume. Scanned image-only PDFs require OCR and are not supported by this prototype.");
        }

        const int maximumCharacters =
            100_000;

        if (text.Length >
            maximumCharacters)
        {
            text =
                text[..maximumCharacters];
        }

        return text;
    }

    private static string ExtractPdf(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var builder =
            new StringBuilder();

        using var document =
            PdfDocument.Open(stream);

        foreach (var page in
            document.GetPages())
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            builder.AppendLine(
                page.Text);
        }

        return builder.ToString();
    }

    private static string ExtractDocx(
        Stream stream)
    {
        using var document =
            WordprocessingDocument.Open(
                stream,
                false);

        return document.MainDocumentPart?
            .Document?
            .Body?
            .InnerText
            ?? string.Empty;
    }

    private static string NormalizeText(
        string value)
    {
        var lines = value
            .Replace("\0", string.Empty)
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n')
            .Select(line =>
                string.Join(
                    " ",
                    line.Split(
                        [' ', '\t'],
                        StringSplitOptions
                            .RemoveEmptyEntries)))
            .Where(line =>
                !string.IsNullOrWhiteSpace(
                    line));

        return string.Join(
                Environment.NewLine,
                lines)
            .Trim();
    }
}
