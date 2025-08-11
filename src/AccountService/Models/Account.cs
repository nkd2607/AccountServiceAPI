using AccountService.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models;

/// <summary>
///     Представляет банковский счёт
/// </summary>
public class Account
{
    /// <summary>
    ///     Уникальный идентификатор счёта
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    ///     Уникальный идентификатор владельца
    /// </summary>
    /// <example>c7e2d8a4-5f1d-4b5e-9e3a-cf47e8b9d8a3</example>
    public Guid OwnerId { get; set; }

    /// <summary>
    ///     Тип счёта (Checking, Deposit, Credit)
    /// </summary>
    /// <example>Checking</example>
    public AccountType Type { get; set; }

    /// <summary>
    ///     Валюта счёта (ISO 4217)
    /// </summary>
    /// <example>USD</example>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Текущий баланс счёта
    /// </summary>
    /// ///
    /// <example>1000.00</example>
    public decimal Balance { get; set; }

    /// <summary>
    ///     Процентная ставка (только для счетов типа Deposit/Credit)
    /// </summary>
    /// <example>1.5</example>
    public decimal? InterestRate { get; set; }

    /// <summary>
    ///     Дата открытия счёта
    /// </summary>
    /// <example>2025-01-15T00:00:00Z</example>
    public DateTime OpeningDate { get; set; }

    /// <summary>
    ///     Дата закрытия счёта (если он закрыт)
    /// </summary>
    /// <example>2026-01-01T00:00:00Z</example>
    public DateTime? ClosingDate { get; set; }

    /// <summary>
    ///     Список транзакций, связанных со счётом
    /// </summary>
    public List<Transaction> Transactions { get; set; } = [];
    
    [Column("xmin", TypeName = "xid")]
    public uint Version { get; set; } 
}