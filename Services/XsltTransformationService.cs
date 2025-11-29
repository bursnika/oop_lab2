using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using LibraryXmlProcessor.Services.Logging;

namespace LibraryXmlProcessor.Services;

public class XsltTransformationService
{
    private readonly EventLogger _logger = EventLogger.Instance;

    public async Task<string> TransformToHtmlAsync(string xmlFilePath, string xsltFilePath, string outputFileName)
    {
        return await Task.Run(() =>
        {
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Exports");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            if (!outputFileName.EndsWith(".html"))
            {
                outputFileName += ".html";
            }

            var outputFile = Path.Combine(outputPath, outputFileName);

            var xslt = new XslCompiledTransform();
            xslt.Load(xsltFilePath);

            using var writer = XmlWriter.Create(outputFile, new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            });

            xslt.Transform(xmlFilePath, writer);

            _logger.Log(LogLevel.Transformation, $"Збережено у файл {outputFileName}");

            return outputFile;
        });
    }

    public string GetTransformedHtmlContent(string xmlFilePath, string xsltFilePath)
    {
        var xslt = new XslCompiledTransform();
        xslt.Load(xsltFilePath);

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = true
        });

        xslt.Transform(xmlFilePath, xmlWriter);

        return stringWriter.ToString();
    }
}
