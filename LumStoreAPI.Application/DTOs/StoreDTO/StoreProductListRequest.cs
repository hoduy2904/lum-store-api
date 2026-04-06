using System.ComponentModel.DataAnnotations;
using LumStoreAPI.Application.DTOs.Responses;

namespace LumStoreAPI.Application.DTOs.StoreDTO
{
    public class StoreProductListRequest : PagingModel
    {
        /// <summary>Full-text search against ProductName and Tags.</summary>
        public string? Search { get; set; }

        /// <summary>Filter by category NodeAlias, e.g. "gel-polish".</summary>
        public string? Category { get; set; }

        public bool? IsNew { get; set; }
        public bool? IsBestSeller { get; set; }

        /// <summary>Filter products where PriceDiscount > 0.</summary>
        public bool? IsSale { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Sort order. Accepted values:
        /// price_asc | price_desc | newest | bestseller | rating | default
        /// </summary>
        public string SortBy { get; set; } = "default";
    }
}
