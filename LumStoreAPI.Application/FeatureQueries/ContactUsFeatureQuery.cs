using System;
using LumStoreAPI.Application.DTOs.QueryDTOs;
using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Interfaces.Sytems;
using MediatR;

namespace LumStoreAPI.Application.FeatureQueries;

[MappingFeatureQuery<ContactUs, ContactUsFeatureQuery>]
public record class ContactUsFeatureQuery(ContactUs ContactUs) : IGenericFeatureQuery, IRequest<ContactUsFeatureDTO>
{
}
