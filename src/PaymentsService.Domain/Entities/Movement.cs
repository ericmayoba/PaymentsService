using PaymentsService.Domain.Enums;

namespace PaymentsService.Domain.Entities;

public class Movement
{
    public int Id { get; set; }
    public int WalletId { get; set; }
    public double Amount { get; set; }
    public MovementType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    public Movement()
    {
    }

    public Movement(int walletId, double amount, MovementType type)
    {
        WalletId = walletId;
        Amount = amount;
        Type = type;
        CreatedAt = DateTime.UtcNow;
    }
}
