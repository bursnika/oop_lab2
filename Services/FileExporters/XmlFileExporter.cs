using System.IO;
using System.Threading.Tasks;

namespace LibraryXmlProcessor.Services.FileExporters;

public class XmlFileExporter : IFileExporter
{
    public async Task<string> ExportAsync(string content, string fileName)
    {
        if (!fileName.EndsWith(".xml", System.StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".xml";
        }

        // Go up from bin/Debug/net9.0 to project root, then to Exports
        var projectRoot = Path.GetFullPath(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        var outputPath = Path.Combine(projectRoot, "Exports");
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        var filePath = Path.Combine(outputPath, fileName);
        await File.WriteAllTextAsync(filePath, content);

        return filePath;
    }

    public string GetFileExtension() => ".xml";

    public string GetExporterName() => "XML Exporter";
}

public class XmlExporterFactory : FileExporterBase
{
    public override IFileExporter CreateExporter()
    {
        return new XmlFileExporter();
    }
}
