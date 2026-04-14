using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Entities.Integrations;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure.Types;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations
{
    internal class ShiprelaySystemRepository(LumStoreContext context) : IShiprelaySystemRespository
    {
        private readonly LumStoreContext _context = context;
        public Task<int> DeleteShiprelayDataAsync(int previousDay = 30)
        {
            return _context.ShiprelayDataSyncs.Where(x => x.UpdatedAt <= DateTime.UtcNow.AddDays(30))
                 .ExecuteDeleteAsync();
        }

        public async Task<IPagedEnumerable<ShiprelayDataSync>> GetShiprelayDataAsync(int page, int pageSize, Expression<Func<ShiprelayDataSync, bool>>? where = null)
        {
            where ??= x => true;
            var data = _context.ShiprelayDataSyncs
                .AsNoTracking()
                .Where(where)
                .Take(pageSize)
                .Skip((page - 1) * pageSize);

            var count = await data.CountAsync();

            return new PagedEnumerable<ShiprelayDataSync>(await data.ToListAsync(), count);
        }

        public async Task SyncProductShiprelayAsync(int productId, EntryActionStatus entryActionStatus, int? variantId = null)
        {
            if (!await UpdateShiprelayDataAsync(productId, variantId, EmailStatus.Waiting, entryActionStatus) && variantId != null)
            {
                _context.Add(new ShiprelayDataSync
                {
                    Status = EmailStatus.Waiting,
                    VariantID = variantId.Value,
                    EntryActionStatus = entryActionStatus
                });

                await _context.SaveChangesAsync();
            }
        }

        public async Task SyncProductShiprelaysAsync(int[] productIds, EntryActionStatus entryActionStatus)
        {
            if (productIds.Length == 0) return;
            var variantIds = await _context
                .ProductVariants
                .AsNoTracking()
                .Where(x => productIds.Contains(x.ProductID))
                .Select(x => x.ItemID).ToArrayAsync();

            await this.SyncVariantShiprelayAsync(variantIds, entryActionStatus);
        }

        public async Task SyncVariantShiprelayAsync(int variantId, EntryActionStatus entryActionStatus)
        {
            var shiprelayItem = await _context.ShiprelayDataSyncs
            .FirstOrDefaultAsync(x => x.VariantID == variantId);
            if (shiprelayItem == null)
            {
                var variant = _context.ProductVariants.Any(x => x.ItemID == variantId);
                if (!variant)
                    return;
                _context.ShiprelayDataSyncs.Add(new ShiprelayDataSync
                {
                    Status = EmailStatus.Waiting,
                    VariantID = variantId,
                    EntryActionStatus = entryActionStatus
                });
            }
            else
            {
                shiprelayItem.Status = EmailStatus.Waiting;
                shiprelayItem.EntryActionStatus = entryActionStatus;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SyncVariantShiprelayAsync(int[] variantIds, EntryActionStatus entryActionStatus)
        {
            if (variantIds.Length == 0) return;

            await _context.ShiprelayDataSyncs
                .Where(x => variantIds.Contains(x.VariantID))
                .ExecuteDeleteAsync();

            _context.ShiprelayDataSyncs.AddRange(variantIds.Select(x => new ShiprelayDataSync
            {
                EntryActionStatus = entryActionStatus,
                VariantID = x,
                Status = EmailStatus.Waiting,
            }));
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateShiprelayDataAsync(int productId, int? variantId, EmailStatus status, EntryActionStatus entryActionStatus, string? message = null)
        {
            var variantIds = _context.ProductVariants
                .Where(x => x.ProductID == productId)
                .AsNoTracking()
                .Select(X => X.ItemID);

            var shiprelaySystem = await _context.ShiprelayDataSyncs
                .Where(x => (variantId != null ? x.VariantID == variantId.Value : variantIds.Contains(x.VariantID)) && x.Status != EmailStatus.Success)
                .ExecuteUpdateAsync(x =>
                x.SetProperty(p => p.UpdatedAt, DateTime.UtcNow)
                .SetProperty(p => p.Status, status)
                .SetProperty(p => p.Message, message)
                .SetProperty(p => p.EntryActionStatus, entryActionStatus)
                .SetProperty(p => p.RunnedAt, p => status == EmailStatus.Waiting ? p.RunnedAt : DateTime.UtcNow));

            return shiprelaySystem > 0;
        }

        public async Task<bool> UpdateShiprelayDataAsync(int itemId, EmailStatus status, EntryActionStatus entryActionStatus, string? message = null)
        {
            var shiprelaySystem = await _context.ShiprelayDataSyncs
                 .Where(x => x.ItemID == itemId)
                 .ExecuteUpdateAsync(x =>
                 x.SetProperty(p => p.UpdatedAt, DateTime.UtcNow)
                 .SetProperty(p => p.Status, status)
                 .SetProperty(p => p.Message, message)
                 .SetProperty(p => p.EntryActionStatus, entryActionStatus)
                 .SetProperty(p => p.RunnedAt, p => status == EmailStatus.Waiting ? p.RunnedAt : DateTime.UtcNow));

            return shiprelaySystem > 0;
        }
    }
}
