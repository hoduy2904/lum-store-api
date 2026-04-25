using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace LumStoreAPI.Infrastructure.Interceptors;

public class ProductInterceptor
(IServiceProvider serviceProvider) : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return await base.SavedChangesAsync(eventData, result, cancellationToken);

        // Only care about entities that were actually mutated (not mere tracked reads)
        var entities = context.ChangeTracker.Entries()
            .Where(x => (x.Entity is Product || x.Entity is ProductVariant) &&
                        x.State != EntityState.Unchanged &&
                        x.State != EntityState.Detached)
            .ToList();

        // Skip scope creation entirely when no product/variant changes are present
        if (!entities.Any())
            return await base.SavedChangesAsync(eventData, result, cancellationToken);

        var scope = _serviceProvider.CreateScope();
        var shiprelaySyncRepository = scope.ServiceProvider.GetRequiredService<IShiprelaySystemRespository>();

        var variants = entities.Where(x => x.Entity is ProductVariant)
            .Select(x => new { Entity = (ProductVariant)x.Entity, x.State })
            .GroupBy(x => x.State)
            .ToList();

        var products = entities
            .Where(x => x.Entity is Product)
            .Select(x => new { Entity = (Product)x.Entity, x.State })
            .GroupBy(x => x.State)
            .ToList();

        if (variants.Any())
        {
            foreach (var variant in variants)
            {
                var variantActions = variant.Select(x => x.Entity.ItemID).ToArray();
                var shiprelayState = variant.Key == EntityState.Deleted ? EntryActionStatus.DELETE : EntryActionStatus.UPDATE;
                await shiprelaySyncRepository
                .SyncVariantShiprelayAsync(variantActions, shiprelayState);
            }
        }
        if (products.Any())
        {
            if (!entities.Where(x => x.Entity is Product).Any(x =>
            {
                return x.Metadata.GetDeclaredProperties().Any(p =>
                 {
                     var propEntry = x.Property(p.Name);
                     return propEntry.CurrentValue != propEntry.OriginalValue;
                 });
            }))
            {
                return await base.SavedChangesAsync(eventData, result, cancellationToken);
            }

            foreach (var product in products)
            {
                var productActions = product.Select(x => x.Entity.NodeID).ToArray();
                var shiprelayState = product.Key == EntityState.Deleted ? EntryActionStatus.DELETE : EntryActionStatus.UPDATE;
                await shiprelaySyncRepository
                .SyncProductShiprelaysAsync(productActions, shiprelayState);
            }
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}
