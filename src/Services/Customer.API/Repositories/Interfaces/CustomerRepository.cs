using Contracts.Common.Interfaces;
using Customer.API.Persistence;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Customer.API.Repositories.Interfaces;

public class CustomerRepository: RepositoryBaseAsync<Entities.Customer,int,CustomerContext>, ICustomerRepository
{
    public CustomerRepository(CustomerContext dbContext, IUnitOfWork<CustomerContext> unitOfWork) : base(dbContext, unitOfWork)
    {
    }

    public Task<Entities.Customer> GetCustomerByUserNameAsync(string username) =>
     FindByCondition(x => x.UserName.Equals(username)).SingleOrDefaultAsync();

    public async Task<IEnumerable<Entities.Customer>> GetCustomersAsync() => await FindAll().ToListAsync();
}