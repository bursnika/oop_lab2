using System.Threading.Tasks;

namespace LibraryXmlProcessor.Services.FileExporters;

public interface IFileExporter
{
    Task<string> ExportAsync(string content, string fileName);
    string GetFileExtension();
    string GetExporterName();
}
