namespace LibraryXmlProcessor.Models;

public class BorrowedBook
{
    public string BorrowId { get; set; } = string.Empty;
    public string ReaderId { get; set; } = string.Empty;
    public string BookId { get; set; } = string.Empty;
    public string BorrowDate { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public string? ReturnDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool? Renewable { get; set; }

    public string Notes { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Borrow #{BorrowId}: Reader {ReaderId} - Book {BookId} ({Status})";
    }
}
