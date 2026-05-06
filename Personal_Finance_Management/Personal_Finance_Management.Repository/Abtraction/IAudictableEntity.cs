namespace Personal_Finance_Management.Repository.Abtraction;

public interface IAudictableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
}