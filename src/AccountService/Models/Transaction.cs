using AccountService.Models.Enums;

namespace AccountService.Models;

/// <summary>
///     Представляет транзакцию между счетами
/// </summary>
public class Transaction
{
    /// <summary>
    ///     Уникальный идентификатор транзакции
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Уникальный идентификатор счёта, связанного с транзакцией
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    ///     Уникальный идентификатор счёта контрагента (если таковой присутствует).
    ///     Null для транзакций без контрагента (например, транзакции с наличными).
    /// </summary>
    public Guid? CounterpartyAccountId { get; set; }

    /// <summary>
    ///     Денежная сумма транзакции
    /// </summary>
    public decimal Sum { get; set; }

    /// <summary>
    ///     Код валюты, используемой в транзакции (ISO 4217).
    /// </summary>
    /// <example>USD</example>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Тип/категория транзакции
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    ///     Пользовательское описание транзакции
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Дата произведения транзакции
    /// </summary>
    public DateTime DateTime { get; set; }
}