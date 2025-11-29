namespace LibraryXmlProcessor.Models;

public class Reader
{
    public string Id { get; set; } = string.Empty;
    public string? RegistrationDate { get; set; }
    public string? Status { get; set; }
    public string? MembershipType { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Faculty { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? Course { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public string FullName => MiddleName != null
        ? $"{LastName} {FirstName} {MiddleName}"
        : $"{LastName} {FirstName}";

    public override string ToString()
    {
        return $"{FullName} - {Position}, {Department}";
    }
}
