using System.Collections.Generic;

namespace LibraryXmlProcessor.Models;

public class SearchCriteria
{
    public Dictionary<string, string> Filters { get; set; } = new();

    public void AddFilter(string key, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            Filters[key] = value;
        }
    }

    public bool HasFilters => Filters.Count > 0;

    public override string ToString()
    {
        if (!HasFilters) return "No filters";

        var filters = new List<string>();
        foreach (var filter in Filters)
        {
            filters.Add($"{filter.Key}={filter.Value}");
        }
        return string.Join(", ", filters);
    }
}
