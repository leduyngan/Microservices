using Contracts.Domains.Interfaces;
using Customer.API.Persistence;

namespace Customer.API.Repositories.Interfaces;

public interface ICustomerRepository: IRepositoryBase<Entities.Customer, int, CustomerContext>
{
    Task<Entities.Customer> GetCustomerByUserNameAsync(string username);
    Task<IEnumerable<Entities.Customer>> GetCustomersAsync();
}