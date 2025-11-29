using System;
using System.Collections.Generic;
using System.Xml;
using LibraryXmlProcessor.Models;

namespace LibraryXmlProcessor.Services.Parsers;

public class DomXmlParser : IXmlParser
{
    public string GetParserName() => "DOM API";

    public List<Book> ParseBooks(string xmlFilePath, SearchCriteria? criteria = null)
    {
        var doc = new XmlDocument();
        doc.Load(xmlFilePath);

        var books = new List<Book>();
        var bookNodes = doc.SelectNodes("//Book");

        if (bookNodes == null) return books;

        foreach (XmlNode bookNode in bookNodes)
        {
            if (bookNode.Attributes == null) continue;

            var book = new Book
            {
                Id = bookNode.Attributes["id"]?.Value ?? string.Empty,
                Isbn = bookNode.Attributes["isbn"]?.Value,
                Year = bookNode.Attributes["year"]?.Value,
                Available = ParseBool(bookNode.Attributes["available"]?.Value),
                Language = bookNode.Attributes["language"]?.Value,
                Edition = bookNode.Attributes["edition"]?.Value,
                Title = bookNode.SelectSingleNode("Title")?.InnerText ?? string.Empty,
                Annotation = bookNode.SelectSingleNode("Annotation")?.InnerText ?? string.Empty,
                Category = bookNode.SelectSingleNode("Category")?.InnerText ?? string.Empty,
                Publisher = bookNode.SelectSingleNode("Publisher")?.InnerText ?? string.Empty,
                Pages = int.TryParse(bookNode.SelectSingleNode("Pages")?.InnerText, out var pages) ? pages : 0
            };

            var authorNodes = bookNode.SelectNodes("Author");
            if (authorNodes != null)
            {
                foreach (XmlNode authorNode in authorNodes)
                {
                    book.Authors.Add(new Author
                    {
                        FirstName = authorNode.SelectSingleNode("FirstName")?.InnerText ?? string.Empty,
                        LastName = authorNode.SelectSingleNode("LastName")?.InnerText ?? string.Empty,
                        MiddleName = authorNode.SelectSingleNode("MiddleName")?.InnerText
                    });
                }
            }

            if (criteria == null || MatchesCriteria(bookNode, criteria))
            {
                books.Add(book);
            }
        }

        return books;
    }

    public List<Reader> ParseReaders(string xmlFilePath, SearchCriteria? criteria = null)
    {
        var doc = new XmlDocument();
        doc.Load(xmlFilePath);

        var readers = new List<Reader>();
        var readerNodes = doc.SelectNodes("//Reader");

        if (readerNodes == null) return readers;

        foreach (XmlNode readerNode in readerNodes)
        {
            if (readerNode.Attributes == null) continue;

            var reader = new Reader
            {
                Id = readerNode.Attributes["id"]?.Value ?? string.Empty,
                RegistrationDate = readerNode.Attributes["registrationDate"]?.Value,
                Status = readerNode.Attributes["status"]?.Value,
                MembershipType = readerNode.Attributes["membershipType"]?.Value,
                FirstName = readerNode.SelectSingleNode("FirstName")?.InnerText ?? string.Empty,
                LastName = readerNode.SelectSingleNode("LastName")?.InnerText ?? string.Empty,
                MiddleName = readerNode.SelectSingleNode("MiddleName")?.InnerText,
                Faculty = readerNode.SelectSingleNode("Faculty")?.InnerText ?? string.Empty,
                Department = readerNode.SelectSingleNode("Department")?.InnerText ?? string.Empty,
                Position = readerNode.SelectSingleNode("Position")?.InnerText ?? string.Empty,
                Course = readerNode.SelectSingleNode("Course")?.InnerText,
                Email = readerNode.SelectSingleNode("Email")?.InnerText ?? string.Empty,
                Phone = readerNode.SelectSingleNode("Phone")?.InnerText
            };

            if (criteria == null || MatchesCriteria(readerNode, criteria))
            {
                readers.Add(reader);
            }
        }

        return readers;
    }

    public List<BorrowedBook> ParseBorrowedBooks(string xmlFilePath, SearchCriteria? criteria = null)
    {
        var doc = new XmlDocument();
        doc.Load(xmlFilePath);

        var borrowedBooks = new List<BorrowedBook>();
        var borrowNodes = doc.SelectNodes("//Borrow");

        if (borrowNodes == null) return borrowedBooks;

        foreach (XmlNode borrowNode in borrowNodes)
        {
            if (borrowNode.Attributes == null) continue;

            var borrow = new BorrowedBook
            {
                BorrowId = borrowNode.Attributes["borrowId"]?.Value ?? string.Empty,
                ReaderId = borrowNode.Attributes["readerId"]?.Value ?? string.Empty,
                BookId = borrowNode.Attributes["bookId"]?.Value ?? string.Empty,
                BorrowDate = borrowNode.Attributes["borrowDate"]?.Value ?? string.Empty,
                DueDate = borrowNode.Attributes["dueDate"]?.Value ?? string.Empty,
                ReturnDate = borrowNode.Attributes["returnDate"]?.Value,
                Status = borrowNode.Attributes["status"]?.Value ?? string.Empty,
                Renewable = ParseBool(borrowNode.Attributes["renewable"]?.Value),
                Notes = borrowNode.SelectSingleNode("Notes")?.InnerText ?? string.Empty
            };

            if (criteria == null || MatchesCriteria(borrowNode, criteria))
            {
                borrowedBooks.Add(borrow);
            }
        }

        return borrowedBooks;
    }

    private bool MatchesCriteria(XmlNode node, SearchCriteria criteria)
    {
        foreach (var filter in criteria.Filters)
        {
            var attrValue = node.Attributes?[filter.Key]?.Value;
            var elemValue = node.SelectSingleNode(filter.Key)?.InnerText;

            var value = attrValue ?? elemValue;

            if (value == null || !value.Contains(filter.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private bool? ParseBool(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        return bool.TryParse(value, out var result) ? result : null;
    }
}
