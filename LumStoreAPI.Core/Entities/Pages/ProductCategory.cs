using System.ComponentModel;
using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;

namespace LumStoreAPI.Core.Entities.Pages
{
    [RegisterPageType(CLASS_NAME, typeof(ProductCategory))]
    public class ProductCategory : DocumentPage
    {
        public const string CLASS_NAME = "Pages.ProductCategory";

        [DocumentName]
        [DisplayName("Category name")]
        public string CategoryName { get; set; } = default!;

        /// <summary>Unsplash/CDN URL or relative path for the category cover image.</summary>
        [DisplayName("Category image URL")]
        public string? CategoryImage { get; set; }

        [DisplayName("Category description")]
        public string? CategoryDescription { get; set; }
    }
}
