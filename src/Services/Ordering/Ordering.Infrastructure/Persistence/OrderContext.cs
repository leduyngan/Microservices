using System.Reflection;
using Contracts.Common.Events;
using Contracts.Domains.Interfaces;
using Infrastructure.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Entities;
using Serilog;

namespace Ordering.Infrastructure.Persistence;

public class OrderContext : DbContext
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;
    public OrderContext(DbContextOptions<OrderContext> options, IMediator mediator, ILogger logger) : base(options)
    {
        _mediator = mediator;
        _logger = logger;
    }
    public DbSet<Order> Orders { get; set; }
    private List<BaseEvent> _baseEvents;

    private void SetBaseEventsBeforeSaveChanges()
    {
        var domainEntities = ChangeTracker.Entries<IEventEntity>()
            .Select(e => e.Entity)
            .Where(x => x.DomainEvents().Any())
            .ToList();
        
        _baseEvents = domainEntities.SelectMany(e => e.DomainEvents()).ToList();
        domainEntities.ForEach(x => x.ClearDomainEvents());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(modelBuilder);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        SetBaseEventsBeforeSaveChanges();
        var modified = ChangeTracker.Entries().Where(e =>
            e.State == EntityState.Modified || e.State == EntityState.Added || e.State == EntityState.Deleted);

        foreach (var item in modified)
        {
            switch (item.State)
            {
                case EntityState.Added:
                    if (item.Entity is IDateTracking addedEntity)
                    {
                        addedEntity.CreatedDate = DateTime.UtcNow;
                        item.State = EntityState.Added;
                    }

                    break;

                case EntityState.Modified:
                    Entry(item.Entity).Property("Id").IsModified = false;
                    if (item.Entity is IDateTracking modifiedEntity)
                    {
                        modifiedEntity.LastModifiedDate = DateTime.UtcNow;
                        item.State = EntityState.Modified;
                    }

                    break;
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);
        _mediator.DispatchDomainEvnetAsync(_baseEvents, _logger);
        
        return result;
    }
}