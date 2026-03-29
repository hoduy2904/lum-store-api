using System.ComponentModel;
using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;

namespace LumStoreAPI.Core.Entities.Pages
{
    [RegisterPageType(CLASS_NAME, typeof(Product))]
    public class ProductCategory : DocumentPage
    {
        public const string CLASS_NAME = "Pages.ProductCategory";
        [DocumentName]
        [DisplayName("Category name")]
        public string CategoryName { get; set; } = default!;
    }
}
