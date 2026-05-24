namespace GameStore.api;

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(decimal amount, string currency);
}

public record PaymentResult(
    bool IsSuccess,
    string? TransactionId,
    string? ErrorMessage
);
