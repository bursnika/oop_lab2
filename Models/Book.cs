using System.Collections.Generic;

namespace LibraryXmlProcessor.Models;

public class Book
{
    public string Id { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public string? Year { get; set; }
    public bool? Available { get; set; }
    public string? Language { get; set; }
    public string? Edition { get; set; }

    public string Title { get; set; } = string.Empty;
    public List<Author> Authors { get; set; } = new();
    public string Annotation { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public int Pages { get; set; }

    public override string ToString()
    {
        var authorsStr = string.Join(", ", Authors);
        return $"{Title} - {authorsStr} ({Year})";
    }
}

public class Author
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }

    public override string ToString()
    {
        return MiddleName != null
            ? $"{FirstName} {MiddleName} {LastName}"
            : $"{FirstName} {LastName}";
    }
}
