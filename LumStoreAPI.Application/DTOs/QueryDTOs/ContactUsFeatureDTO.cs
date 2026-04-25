using System;
using LumStoreAPI.Application.FeatureQueries;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Models.Systems.SettingKeys;
using MediatR;

namespace LumStoreAPI.Application.DTOs.QueryDTOs;

public record class ContactUsFeatureDTO(
    string PageTitle,
    string? Description,
    IEnumerable<StoreHoursSetting> StoreHoursSettings,
    IEnumerable<ContactInformationSetting> ContactInformationSettings
);
