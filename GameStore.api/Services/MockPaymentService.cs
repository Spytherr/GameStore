namespace GameStore.api;

public class MockPaymentService : IPaymentService
{
    public Task<PaymentResult> ProcessPaymentAsync(decimal amount, string currency)
    {
        var transactionId = $"MOCK-{Guid.NewGuid():N}"[..20];
        return Task.FromResult(new PaymentResult(true, transactionId, null));
    }
}
