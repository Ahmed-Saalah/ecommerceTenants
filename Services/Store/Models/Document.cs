namespace Store.Core.Models;

public sealed class Document
{
    public int DocumentId { get; set; }
    public int StoreId { get; set; }
    public string? DocumentType { get; set; }
    public string? FilePath { get; set; }
    public DateTime Timestamp { get; set; }
    public Store Store { get; set; }
}
