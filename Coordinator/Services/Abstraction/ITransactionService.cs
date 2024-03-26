namespace Coordinator.Services.Abstraction
{
    public interface ITransactionService
    {
        Task<Guid> CreateTransactionAsync();
        Task PreapareServicesAsync(Guid transactionId);
        Task<bool> CheckReadyServicesAsync(Guid TransactionId);
        Task CommitAsync(Guid transactionId);
        Task<bool> CheckTransactionStateServicesAsync(Guid transactionId);
        Task RollBackAsync(Guid transactionId);
    }
}
