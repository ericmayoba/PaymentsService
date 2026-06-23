namespace PaymentsService.Domain.Entities;

public class Wallet
{
    public int Id { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
