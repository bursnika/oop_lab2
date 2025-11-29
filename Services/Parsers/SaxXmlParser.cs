using System;
using System.Collections.Generic;
using System.Xml;
using LibraryXmlProcessor.Models;

namespace LibraryXmlProcessor.Services.Parsers;

public class SaxXmlParser : IXmlParser
{
    public string GetParserName() => "SAX API (XmlReader)";

    public List<Book> ParseBooks(string xmlFilePath, SearchCriteria? criteria = null)
    {
        var books = new List<Book>();

        using var reader = XmlReader.Create(xmlFilePath);
        Book? currentBook = null;
        Author? currentAuthor = null;
        string? currentElement = null;

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentElement = reader.Name;

                    if (reader.Name == "Book")
                    {
                        currentBook = new Book
                        {
                            Id = reader.GetAttribute("id") ?? string.Empty,
                            Isbn = reader.GetAttribute("isbn"),
                            Year = reader.GetAttribute("year"),
                            Available = ParseBool(reader.GetAttribute("available")),
                            Language = reader.GetAttribute("language"),
                            Edition = reader.GetAttribute("edition")
                        };
                    }
                    else if (reader.Name == "Author" && currentBook != null)
                    {
                        currentAuthor = new Author();
                    }
                    break;

                case XmlNodeType.Text:
                    if (currentBook == null) break;

                    if (currentAuthor != null)
                    {
                        switch (currentElement)
                        {
                            case "FirstName":
                                currentAuthor.FirstName = reader.Value;
                                break;
                            case "LastName":
                                currentAuthor.LastName = reader.Value;
                                break;
                            case "MiddleName":
                                currentAuthor.MiddleName = reader.Value;
                                break;
                        }
                    }
                    else
                    {
                        switch (currentElement)
                        {
                            case "Title":
                                currentBook.Title = reader.Value;
                                break;
                            case "Annotation":
                                currentBook.Annotation = reader.Value;
                                break;
                            case "Category":
                                currentBook.Category = reader.Value;
                                break;
                            case "Publisher":
                                currentBook.Publisher = reader.Value;
                                break;
                            case "Pages":
                                currentBook.Pages = int.TryParse(reader.Value, out var pages) ? pages : 0;
                                break;
                        }
                    }
                    break;

                case XmlNodeType.EndElement:
                    if (reader.Name == "Author" && currentAuthor != null && currentBook != null)
                    {
                        currentBook.Authors.Add(currentAuthor);
                        currentAuthor = null;
                    }
                    else if (reader.Name == "Book" && currentBook != null)
                    {
                        if (criteria == null || MatchesCriteria(currentBook, criteria))
                        {
                            books.Add(currentBook);
                        }
                        currentBook = null;
                    }
                    break;
            }
        }

        return books;
    }

    public List<Reader> ParseReaders(string xmlFilePath, SearchCriteria? criteria = null)
    {
        var readers = new List<Reader>();

        using var reader = XmlReader.Create(xmlFilePath);
        Reader? currentReader = null;
        string? currentElement = null;

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentElement = reader.Name;

                    if (reader.Name == "Reader")
                    {
                        currentReader = new Reader
                        {
                            Id = reader.GetAttribute("id") ?? string.Empty,
                            RegistrationDate = reader.GetAttribute("registrationDate"),
                            Status = reader.GetAttribute("status"),
                            MembershipType = reader.GetAttribute("membershipType")
                        };
                    }
                    break;

                case XmlNodeType.Text:
                    if (currentReader == null) break;

                    switch (currentElement)
                    {
                        case "FirstName":
                            currentReader.FirstName = reader.Value;
                            break;
                        case "LastName":
                            currentReader.LastName = reader.Value;
                            break;
                        case "MiddleName":
                            currentReader.MiddleName = reader.Value;
                            break;
                        case "Faculty":
                            currentReader.Faculty = reader.Value;
                            break;
                        case "Department":
                            currentReader.Department = reader.Value;
                            break;
                        case "Position":
                            currentReader.Position = reader.Value;
                            break;
                        case "Course":
                            currentReader.Course = reader.Value;
                            break;
                        case "Email":
                            currentReader.Email = reader.Value;
                            break;
                        case "Phone":
                            currentReader.Phone = reader.Value;
                            break;
                    }
                    break;

                case XmlNodeType.EndElement:
                    if (reader.Name == "Reader" && currentReader != null)
                    {
                        if (criteria == null || MatchesCriteria(currentReader, criteria))
                        {
                            readers.Add(currentReader);
                        }
                        currentReader = null;
                    }
                    break;
            }
        }

        return readers;
    }

    public List<BorrowedBook> ParseBorrowedBooks(string xmlFilePath, SearchCriteria? criteria = null)
    {
        var borrowedBooks = new List<BorrowedBook>();

        using var reader = XmlReader.Create(xmlFilePath);
        BorrowedBook? currentBorrow = null;
        string? currentElement = null;

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentElement = reader.Name;

                    if (reader.Name == "Borrow")
                    {
                        currentBorrow = new BorrowedBook
                        {
                            BorrowId = reader.GetAttribute("borrowId") ?? string.Empty,
                            ReaderId = reader.GetAttribute("readerId") ?? string.Empty,
                            BookId = reader.GetAttribute("bookId") ?? string.Empty,
                            BorrowDate = reader.GetAttribute("borrowDate") ?? string.Empty,
                            DueDate = reader.GetAttribute("dueDate") ?? string.Empty,
                            ReturnDate = reader.GetAttribute("returnDate"),
                            Status = reader.GetAttribute("status") ?? string.Empty,
                            Renewable = ParseBool(reader.GetAttribute("renewable"))
                        };
                    }
                    break;

                case XmlNodeType.Text:
                    if (currentBorrow == null) break;

                    if (currentElement == "Notes")
                    {
                        currentBorrow.Notes = reader.Value;
                    }
                    break;

                case XmlNodeType.EndElement:
                    if (reader.Name == "Borrow" && currentBorrow != null)
                    {
                        if (criteria == null || MatchesCriteria(currentBorrow, criteria))
                        {
                            borrowedBooks.Add(currentBorrow);
                        }
                        currentBorrow = null;
                    }
                    break;
            }
        }

        return borrowedBooks;
    }

    private bool MatchesCriteria(Book book, SearchCriteria criteria)
    {
        foreach (var filter in criteria.Filters)
        {
            var value = filter.Key switch
            {
                "id" => book.Id,
                "isbn" => book.Isbn,
                "year" => book.Year,
                "available" => book.Available?.ToString(),
                "language" => book.Language,
                "edition" => book.Edition,
                "Title" => book.Title,
                "Category" => book.Category,
                "Publisher" => book.Publisher,
                _ => null
            };

            if (value == null || !value.Contains(filter.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private bool MatchesCriteria(Reader readerObj, SearchCriteria criteria)
    {
        foreach (var filter in criteria.Filters)
        {
            var value = filter.Key switch
            {
                "id" => readerObj.Id,
                "status" => readerObj.Status,
                "membershipType" => readerObj.MembershipType,
                "Faculty" => readerObj.Faculty,
                "Department" => readerObj.Department,
                "Position" => readerObj.Position,
                _ => null
            };

            if (value == null || !value.Contains(filter.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private bool MatchesCriteria(BorrowedBook borrow, SearchCriteria criteria)
    {
        foreach (var filter in criteria.Filters)
        {
            var value = filter.Key switch
            {
                "borrowId" => borrow.BorrowId,
                "readerId" => borrow.ReaderId,
                "bookId" => borrow.BookId,
                "status" => borrow.Status,
                _ => null
            };

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
