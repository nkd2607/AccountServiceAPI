namespace AccountService.Domain.Events
{
    public record AccountOpened(
        Guid EventId,
        DateTime OccurredAt,
        Guid AccountId,
        Guid OwnerId,
        string Currency,
        string Type
    );

    public record MoneyCredited(
        Guid EventId,
        DateTime OccurredAt,
        Guid AccountId,
        decimal Amount,
        string Currency,
        Guid OperationId
    );

    public record MoneyDebited(
        Guid EventId,
        DateTime OccurredAt,
        Guid AccountId,
        decimal Amount,
        string Currency,
        Guid OperationId,
        string Reason
    );

    public record TransferCompleted(
        Guid EventId,
        DateTime OccurredAt,
        Guid SourceAccountId,
        Guid DestinationAccountId,
        decimal Amount,
        string Currency,
        Guid TransferId
    );

    public record InterestAccrued(
        Guid EventId,
        DateTime OccurredAt,
        Guid AccountId,
        DateTime PeriodFrom,
        DateTime PeriodTo,
        decimal Amount
    );

    // Потребляемые события
    public record ClientBlocked(
        Guid EventId,
        DateTime OccurredAt,
        Guid ClientId
    );

    public record ClientUnblocked(
        Guid EventId,
        DateTime OccurredAt,
        Guid ClientId
    );
}