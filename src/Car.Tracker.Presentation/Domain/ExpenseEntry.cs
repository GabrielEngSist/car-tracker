namespace Car.Tracker.Presentation.Domain;

public enum ExpenseEntryType
{
    Service = 1,
    Part = 2,
}

public sealed class ExpenseEntry : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CarId { get; set; }
    public ExpenseEntryType Type { get; set; }

    public required string Title { get; set; }
    public decimal Price { get; set; }

    public string? SupplierBrand { get; set; }
    public string? ProductModel { get; set; }

    public DateOnly PerformedAt { get; set; }
    public int KmAtService { get; set; }

    public string? Notes { get; set; }
}
