namespace Store.Core.Models;

public sealed class Store
{
    public int StoreId { get; set; }
    public string StoreName { get; set; }
    public string StoreUrl { get; set; }
    public string? LogoPath { get; set; }
    public string OwnerName { get; set; }
    public string OwnerEmail { get; set; }
    public string OwnerPhoneNumber { get; set; }
    public string ContactUsEmail { get; set; }
    public DateTime Timestamp { get; set; }

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
