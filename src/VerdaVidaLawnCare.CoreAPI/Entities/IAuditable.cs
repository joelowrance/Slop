namespace VerdaVidaLawnCare.CoreAPI.Entities;

public interface IAuditable
{
    public DateTimeOffset CreatedAt { get; internal set; }
    public DateTimeOffset UpdatedAt { get; internal set; }
}
