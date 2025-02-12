namespace Customer.API.Services.Interfaces;

public interface ICustomerService
{
    Task<IResult> GetCustomersByUsernameAsync(string username);
    Task<IResult> GetCustomersAsync();
}