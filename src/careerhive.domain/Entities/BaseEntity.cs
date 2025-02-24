namespace careerhive.domain.Entities;

public abstract class BaseEntity<T>
{
    public virtual T? Id { get; set; }
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
