using System.Runtime.CompilerServices;
using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace LumStoreAPI.Infrastructure.Interceptors;

public class ProductInterceptor(IServiceProvider serviceProvider) : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private static readonly ConditionalWeakTable<DbContext, List<SyncTrackerItem>> _stateTable = new();

    private class SyncTrackerItem
    {
        public object Entity { get; set; } = null!;
        public EntryActionStatus Action { get; set; }
        public bool IsVariant { get; set; }
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        context.ChangeTracker.DetectChanges();

        var entities = context.ChangeTracker.Entries()
            .Where(x => (x.Entity is Product || x.Entity is ProductVariant) &&
                        x.State != EntityState.Unchanged &&
                        x.State != EntityState.Detached)
            .ToList();

        if (entities.Any())
        {
            var trackedList = _stateTable.GetOrCreateValue(context);
            trackedList.Clear();

            foreach (var entry in entities)
            {
                var state = entry.State;
                var shiprelayState = state == EntityState.Deleted ? EntryActionStatus.DELETE : EntryActionStatus.UPDATE;

                if (entry.Entity is ProductVariant variant)
                {
                    trackedList.Add(new SyncTrackerItem { Entity = variant, Action = shiprelayState, IsVariant = true });
                }
                else if (entry.Entity is Product product)
                {
                    if (state == EntityState.Modified)
                    {
                        bool hasRealChanges = entry.Properties.Any(p => p.IsModified && !Equals(p.CurrentValue, p.OriginalValue));
                        if (!hasRealChanges) continue;
                    }

                    trackedList.Add(new SyncTrackerItem { Entity = product, Action = shiprelayState, IsVariant = false });
                }
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return await base.SavedChangesAsync(eventData, result, cancellationToken);

        if (!_stateTable.TryGetValue(context, out var trackedItems) || !trackedItems.Any())
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var shiprelaySyncRepository = scope.ServiceProvider.GetRequiredService<IShiprelaySystemRespository>();

            var variants = trackedItems.Where(x => x.IsVariant).GroupBy(x => x.Action).ToList();
            if (variants.Any())
            {
                foreach (var variant in variants)
                {
                    var variantActions = variant.Select(x => ((ProductVariant)x.Entity).ItemID).ToArray();
                    await shiprelaySyncRepository.SyncVariantShiprelayAsync(variantActions, variant.Key);
                }
            }

            var products = trackedItems.Where(x => !x.IsVariant).GroupBy(x => x.Action).ToList();
            if (products.Any())
            {
                foreach (var product in products)
                {
                    var productActions = product.Select(x => ((Product)x.Entity).NodeID).ToArray();
                    await shiprelaySyncRepository.SyncProductShiprelaysAsync(productActions, product.Key);
                }
            }
        }
        finally
        {
            _stateTable.Remove(context);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}