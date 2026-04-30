namespace Car.Tracker.Api.Domain;

public sealed class MaintenancePlanItem : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CarId { get; set; }

    public required string Title { get; set; }

    public int? DueKmInterval { get; set; }

    /// <summary>
    /// Simple time-based interval (days) so we can support "6 months" (≈ 182 days) etc.
    /// </summary>
    public int? DueTimeIntervalDays { get; set; }

    public bool Active { get; set; } = true;
}
