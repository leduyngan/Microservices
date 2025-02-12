using Customer.API.Repositories.Interfaces;

namespace Customer.API.Services.Interfaces;

public class CustomerService: ICustomerService
{
    private readonly ICustomerRepository _repository;
    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<IResult> GetCustomersByUsernameAsync(string username) =>
        Results.Ok(await _repository.GetCustomerByUserNameAsync(username));

    public async Task<IResult> GetCustomersAsync() => Results.Ok(await _repository.GetCustomersAsync());
}