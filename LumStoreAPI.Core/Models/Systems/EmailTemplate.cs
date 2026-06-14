using System;

namespace LumStoreAPI.Core.Models.Systems;

public class EmailTemplate
{
    public string EmailHeader { get; set; } = default!;
    public string EmailBody { get; set; } = default!;
}
