using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;

namespace LumStoreAPI.Core.Entities.Pages;

public class ContactUs : DocumentPage
{
    public const string CLASS_NAME = "Pages.ContactUs";
    [DocumentName]
    public string Title { get; set; } = default!;
    public string? Descrition { get; set; }
}
