using System;
using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Application.DTOs.DocumentPageDTO;

public record class DocumentPageWidgetRequestDTO
{
    public WidgetData<object>[] Data { get; set; } = [];
}
