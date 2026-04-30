using System;
using LumStoreAPI.Application.DTOs.Widgets;
using LumStoreAPI.Core.Attributes;
using MediatR;

namespace LumStoreAPI.Application.Widgets;

[RegisterWidget("productColorWidget", typeof(ProductColorWidget))]
public record class ProductColorWidget(int MaxColor) : IRequest<ProductColorWidgetDTO>;
