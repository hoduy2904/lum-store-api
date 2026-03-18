using System;
using LumStoreAPI.Application.DTOs.Responses;

namespace LumStoreAPI.Application.DTOs.MediaDTO;

public class MediaItemListingRequest : PagingModel
{
    public string? Search { get; set; }
    public string? Extensions { get; set; }
    public int CategoryID { get; set; }
}
