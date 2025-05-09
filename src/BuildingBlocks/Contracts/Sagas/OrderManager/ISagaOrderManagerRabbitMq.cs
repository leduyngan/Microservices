namespace Contracts.Sagas.OrderManager;

public interface ISagaOrderManagerRabbitMq<in TInput, TOutput> where TInput : class 
    where TOutput : class 
{
    public Task<TOutput> CreateOrder(TInput input);
    public Task<TOutput> RollbackOrder(string username, string documentNo, long orderId);
}