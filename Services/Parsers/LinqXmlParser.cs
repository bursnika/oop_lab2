using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LibraryXmlProcessor.Models;

namespace LibraryXmlProcessor.Services.Parsers;

public class LinqXmlParser : IXmlParser
{
    public string GetParserName() => "LINQ to XML";

    public List<Book> ParseBooks(string xmlFilePath, SearchCriteria? criteria = null)
    {
        var doc = XDocument.Load(xmlFilePath);
        var books = new List<Book>();

        var bookElements = doc.Descendants("Book");

        foreach (var bookElement in bookElements)
        {
            var book = new Book
            {
                Id = bookElement.Attribute("id")?.Value ?? string.Empty,
                Isbn = bookElement.Attribute("isbn")?.Value,
                Year = bookElement.Attribute("year")?.Value,
                Available = ParseBool(bookElement.Attribute("available")?.Value),
                Language = bookElement.Attribute("language")?.Value,
                Edition = bookElement.Attribute("edition")?.Value,
                Title = bookElement.Element("Title")?.Value ?? string.Empty,
                Annotation = bookElement.Element("Annotation")?.Value ?? string.Empty,
                Category = bookElement.Element("Category")?.Value ?? string.Empty,
                Publisher = bookElement.Element("Publisher")?.Value ?? string.Empty,
                Pages = int.TryParse(bookElement.Element("Pages")?.Value, out var pages) ? pages : 0
            };

            foreach (var authorElement in bookElement.Elements("Author"))
            {
                book.Authors.Add(new Author
                {
                    FirstName = authorElement.Element("FirstName")?.Value ?? string.Empty,
                    LastName = authorElement.Element("LastName")?.Value ?? string.Empty,
                    MiddleName = authorElement.Element("MiddleName")?.Value
                });
            }

            if (criteria == null || MatchesCriteria(bookElement, criteria))
            {
                books.Add(book);
            }
        }

        return books;
    }

    public List<Reader> ParseReaders(string xmlFilePath, SearchCriteria? criteria = null)
    {
        var doc = XDocument.Load(xmlFilePath);
        var readers = new List<Reader>();

        var readerElements = doc.Descendants("Reader");

        foreach (var readerElement in readerElements)
        {
            var reader = new Reader
            {
                Id = readerElement.Attribute("id")?.Value ?? string.Empty,
                RegistrationDate = readerElement.Attribute("registrationDate")?.Value,
                Status = readerElement.Attribute("status")?.Value,
                MembershipType = readerElement.Attribute("membershipType")?.Value,
                FirstName = readerElement.Element("FirstName")?.Value ?? string.Empty,
                LastName = readerElement.Element("LastName")?.Value ?? string.Empty,
                MiddleName = readerElement.Element("MiddleName")?.Value,
                Faculty = readerElement.Element("Faculty")?.Value ?? string.Empty,
                Department = readerElement.Element("Department")?.Value ?? string.Empty,
                Position = readerElement.Element("Position")?.Value ?? string.Empty,
                Course = readerElement.Element("Course")?.Value,
                Email = readerElement.Element("Email")?.Value ?? string.Empty,
                Phone = readerElement.Element("Phone")?.Value
            };

            if (criteria == null || MatchesCriteria(readerElement, criteria))
            {
                readers.Add(reader);
            }
        }

        return readers;
    }

    public List<BorrowedBook> ParseBorrowedBooks(string xmlFilePath, SearchCriteria? criteria = null)
    {
        var doc = XDocument.Load(xmlFilePath);
        var borrowedBooks = new List<BorrowedBook>();

        var borrowElements = doc.Descendants("Borrow");

        foreach (var borrowElement in borrowElements)
        {
            var borrow = new BorrowedBook
            {
                BorrowId = borrowElement.Attribute("borrowId")?.Value ?? string.Empty,
                ReaderId = borrowElement.Attribute("readerId")?.Value ?? string.Empty,
                BookId = borrowElement.Attribute("bookId")?.Value ?? string.Empty,
                BorrowDate = borrowElement.Attribute("borrowDate")?.Value ?? string.Empty,
                DueDate = borrowElement.Attribute("dueDate")?.Value ?? string.Empty,
                ReturnDate = borrowElement.Attribute("returnDate")?.Value,
                Status = borrowElement.Attribute("status")?.Value ?? string.Empty,
                Renewable = ParseBool(borrowElement.Attribute("renewable")?.Value),
                Notes = borrowElement.Element("Notes")?.Value ?? string.Empty
            };

            if (criteria == null || MatchesCriteria(borrowElement, criteria))
            {
                borrowedBooks.Add(borrow);
            }
        }

        return borrowedBooks;
    }

    private bool MatchesCriteria(XElement element, SearchCriteria criteria)
    {
        foreach (var filter in criteria.Filters)
        {
            var attrValue = element.Attribute(filter.Key)?.Value;
            var elemValue = element.Element(filter.Key)?.Value;

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
