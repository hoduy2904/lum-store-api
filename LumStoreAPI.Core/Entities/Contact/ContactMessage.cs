using LumStoreAPI.Core.Entities.Base;

namespace LumStoreAPI.Core.Entities.Contact;

public class ContactMessage : BaseClassItem
{
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Message { get; set; } = default!;
    public bool IsRead { get; set; } = false;
}
