using AccountService.Models.Enums;

namespace AccountService.Models;

public class Account
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public AccountType Type { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal? InterestRate { get; set; } 
    public DateTime OpeningDate { get; set; }
    private DateTime? _closingDate;
    public DateTime? ClosingDate
    {
        get => _closingDate;
        set
        {
            if (value.HasValue && value < OpeningDate) throw new Exception("ƒата закрыти€ не может быть раньше даты открыти€");
            _closingDate = value;
        }
    }
    public List<Transaction> Transactions { get; set; } = [];
}