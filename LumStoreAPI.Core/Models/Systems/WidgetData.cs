using System;

namespace LumStoreAPI.Core.Models.Systems;

public class WidgetData<T>
{
    public string WidgetCode { get; set; } = default!;
    public Guid WidgetId { get; set; }
    public T? Properties { get; set; }
}
