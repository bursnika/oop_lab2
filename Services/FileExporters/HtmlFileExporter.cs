using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace LibraryXmlProcessor.Services.FileExporters;

public class HtmlFileExporter : IFileExporter
{
    private readonly string _xsltFilePath;

    public HtmlFileExporter(string xsltFilePath)
    {
        _xsltFilePath = xsltFilePath;
    }

    public async Task<string> ExportAsync(string xmlContent, string fileName)
    {
        if (!fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".html";
        }

        var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Exports");
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        var filePath = Path.Combine(outputPath, fileName);

        await Task.Run(() =>
        {
            var xslt = new XslCompiledTransform();
            xslt.Load(_xsltFilePath);

            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader);
            using var writer = XmlWriter.Create(filePath, new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            });

            xslt.Transform(xmlReader, writer);
        });

        return filePath;
    }

    public string GetFileExtension() => ".html";

    public string GetExporterName() => "HTML Exporter";
}

public class HtmlExporterFactory : FileExporterBase
{
    private readonly string _xsltFilePath;

    public HtmlExporterFactory(string xsltFilePath)
    {
        _xsltFilePath = xsltFilePath;
    }

    public override IFileExporter CreateExporter()
    {
        return new HtmlFileExporter(_xsltFilePath);
    }
}
