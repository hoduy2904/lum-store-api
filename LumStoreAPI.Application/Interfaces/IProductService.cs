using System;
using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.ProductDTO;

namespace LumStoreAPI.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<DocumentClientGetDTO>> GetFeatureProducts(int topN);
    Task<IEnumerable<DocumentClientGetDTO>> GetNewProducts(int topN);
}
