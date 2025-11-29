using System;
using System.IO;
using System.Threading.Tasks;

namespace LibraryXmlProcessor.Services.FileExporters;

public abstract class FileExporterBase
{
    protected string LoadedXmlContent { get; private set; } = string.Empty;
    protected string LoadedXmlFilePath { get; private set; } = string.Empty;

    public void LoadXmlFile(string xmlFilePath)
    {
        if (!File.Exists(xmlFilePath))
        {
            throw new FileNotFoundException($"XML file not found: {xmlFilePath}");
        }

        LoadedXmlFilePath = xmlFilePath;
        LoadedXmlContent = File.ReadAllText(xmlFilePath);
    }

    public abstract IFileExporter CreateExporter();

    public async Task<string> ExportAsync(string fileName)
    {
        if (string.IsNullOrEmpty(LoadedXmlContent))
        {
            throw new InvalidOperationException("No XML content loaded. Call LoadXmlFile first.");
        }

        var exporter = CreateExporter();
        return await exporter.ExportAsync(LoadedXmlContent, fileName);
    }
}
