using LumStoreAPI.Core.Entities.Integrations;
using LumStoreAPI.Core.Entities.Pages;
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

        public async Task SyncProductShiprelayAsync(Product product)
        {
            var variants = await _context
                .ProductVariants
                .Where(x => x.ProductID == product.NodeID)
                .AsNoTracking()
                .Select(x => x.ItemID)
                .ToListAsync();

            if (variants.Count == 0)
                return;

            await _context
                 .ShiprelayDataSyncs
                 .Where(x => variants.Contains(x.VariantID) && x.ProductNodeID == product.NodeID && x.Status != EmailStatus.Success)
                 .ExecuteDeleteAsync();

            await _context.ShiprelayDataSyncs.AddRangeAsync(variants.Select(x => new ShiprelayDataSync
            {
                ProductNodeID = product.NodeID,
                VariantID = x,
                Status = EmailStatus.Waiting
            }));
        }

        public async Task SyncProductShiprelayAsync(int productId, int? variantId = null)
        {
            if (!(await UpdateShiprelayDataAsync(productId, variantId, EmailStatus.Waiting)) && variantId != null)
            {
                _context.Add(new ShiprelayDataSync
                {
                    Status = EmailStatus.Waiting,
                    VariantID = variantId.Value,
                    ProductNodeID = productId
                });

                await _context.SaveChangesAsync();
            }
        }

        public async Task SyncVariantShiprelayAsync(int variantId)
        {
            var shiprelayItem = await _context.ShiprelayDataSyncs.FirstOrDefaultAsync(x => x.VariantID == variantId && x.Status != EmailStatus.Success);
            if (shiprelayItem == null)
            {
                var variant = await _context.ProductVariants.FirstOrDefaultAsync(x => x.ItemID == variantId);
                if (variant == null)
                    return;
                _context.ShiprelayDataSyncs.Add(new ShiprelayDataSync
                {
                    ProductNodeID = variant.ProductID,
                    Status = EmailStatus.Waiting,
                    VariantID = variantId
                });
            }
            else
            {
                shiprelayItem.Status = EmailStatus.Waiting;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateShiprelayDataAsync(int productId, int? variantId, EmailStatus status, string? message = null)
        {
            var shiprelaySystem = await _context.ShiprelayDataSyncs
                .Where(x => x.ProductNodeID == productId && (variantId != null ? x.VariantID == variantId.Value : true) && x.Status != EmailStatus.Success)
                .ExecuteUpdateAsync(x =>
                x.SetProperty(p => p.UpdatedAt, DateTime.UtcNow)
                .SetProperty(p => p.Status, status)
                .SetProperty(p => p.Message, message)
                .SetProperty(p => p.RunnedAt, p => status == EmailStatus.Waiting ? p.RunnedAt : DateTime.UtcNow));

            return shiprelaySystem > 0;
        }
    }
}
