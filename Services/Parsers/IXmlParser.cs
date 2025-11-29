using System.Collections.Generic;
using LibraryXmlProcessor.Models;

namespace LibraryXmlProcessor.Services.Parsers;

public interface IXmlParser
{
    List<Book> ParseBooks(string xmlFilePath, SearchCriteria? criteria = null);
    List<Reader> ParseReaders(string xmlFilePath, SearchCriteria? criteria = null);
    List<BorrowedBook> ParseBorrowedBooks(string xmlFilePath, SearchCriteria? criteria = null);
    string GetParserName();
}
