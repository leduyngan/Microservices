using Common.Logging;
using Contracts.Domains.Interfaces;
using Customer.API.Context;
using Customer.API.Controllers;
using Customer.API.Extensions;
using Customer.API.Persistence;
using Customer.API.Repositories.Interfaces;
using Customer.API.Services.Interfaces;
using HealthChecks.UI.Client;
using Infrastructure.Common;
using Infrastructure.Common.Interfaces;
using Infrastructure.Middlewares;
using Infrastructure.ScheduledJobs;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(Serilogger.Configure);

Log.Information("Starting Customer API up");

try
{
    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddConfigurationSettings(builder.Configuration);
    builder.Services.AddSwaggerGen();
    builder.Services.ConfigureCustomerContext();
    builder.Services.AddInfrastructureServices();
    builder.Services.AddInfraHangfireService();

    // builder.Services.AddScoped(typeof(IRepositoryBase<,,>), typeof(RepositoryBase<,,>))
    //     .AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>))
    //     .AddScoped<ICustomerRepository, CustomerRepository>()
    //     .AddScoped<ICustomerService, CustomerService>();

    var app = builder.Build();

    app.MapGet("/", () => "Welcome to Customer Minimal API!");
    app.MapGet("/api/customers", async (ICustomerService customerService) => await customerService.GetCustomersAsync());
    app.MapCustomersAPI();
    app.MapPost("/api/customers",
        async (Customer.API.Entities.Customer customer, ICustomerRepository customerRepository) =>
        {
            await customerRepository.CreateAsync(customer);
            //await customerRepository.SaveChangesAsync();
        });
    app.MapDelete("/api/customers/{id}",
        async (int id, ICustomerRepository customerRepository) =>
        {
            var customer = await customerRepository.GetByIdAsync(id);
            if (customer != null) return Results.NotFound();
            await customerRepository.DeleteAsync(customer);
            return Results.NoContent();
        });


    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json",
            $"{builder.Environment.ApplicationName} v1"));
    }

    // app.UseHttpsRedirection(); //production only
    
    app.UseMiddleware<ErrorWrappingMiddleware>();
    app.UseAuthorization();
    app.UseHangfireDashboard(builder.Configuration);
    app.MapControllers();

    app.SeedCustomerData();
    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        endpoints.MapDefaultControllerRoute();
    });

    app.Run();
}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal)) throw;

    Log.Fatal(ex, $"Unhandled exception: {ex.Message}");
}
finally
{
    Log.Information("Shut down Customer API copmlete");
    Log.CloseAndFlush();
}