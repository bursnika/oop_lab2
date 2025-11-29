using System.Collections.Generic;
using LibraryXmlProcessor.Models;

namespace LibraryXmlProcessor.Services.Parsers;

public class XmlParserContext
{
    private IXmlParser _parser;

    public XmlParserContext(IXmlParser parser)
    {
        _parser = parser;
    }

    public void SetParser(IXmlParser parser)
    {
        _parser = parser;
    }

    public IXmlParser GetCurrentParser() => _parser;

    public List<Book> ParseBooks(string xmlFilePath, SearchCriteria? criteria = null)
    {
        return _parser.ParseBooks(xmlFilePath, criteria);
    }

    public List<Reader> ParseReaders(string xmlFilePath, SearchCriteria? criteria = null)
    {
        return _parser.ParseReaders(xmlFilePath, criteria);
    }

    public List<BorrowedBook> ParseBorrowedBooks(string xmlFilePath, SearchCriteria? criteria = null)
    {
        return _parser.ParseBorrowedBooks(xmlFilePath, criteria);
    }
}
