using System;
using LumStoreAPI.Application.DTOs.QueryDTOs;
using LumStoreAPI.Application.FeatureQueries;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Systems.SettingKeys;
using MediatR;

namespace LumStoreAPI.Features;

public class ContactUsFeatureHandle(
    ISettingKeyValueService settingKeyValueService,
    ICacheService cacheService

) : IRequestHandler<ContactUsFeatureQuery, ContactUsFeatureDTO>
{
    private readonly ISettingKeyValueService _settingKeyValueService = settingKeyValueService;
    private readonly ICacheService _cacheService = cacheService;
    public async Task<ContactUsFeatureDTO> Handle(ContactUsFeatureQuery request, CancellationToken cancellationToken)
    {
        var contactUsInformations = await _cacheService.GetCacheAsync(
            () => _settingKeyValueService.GetSystemSettingsAsync(
                    SystemSettingKeyConstants.SYSTEM_STORE_HOURS,
                    SystemSettingKeyConstants.CONTACT_INFORMATION),
            builder => builder.Dependencies(dep => dep
                .SettingKey(SystemSettingKeyConstants.SYSTEM_STORE_HOURS)
                .SettingKey(SystemSettingKeyConstants.CONTACT_INFORMATION)
                ).Key("contactus-information").Expiration(0)) ?? [];

        var storeHours = contactUsInformations
            .FirstOrDefault(x => x.Key == SystemSettingKeyConstants.SYSTEM_STORE_HOURS)
            ?.Value as IEnumerable<StoreHoursSetting>;
        var contactInformation = contactUsInformations
            .FirstOrDefault(x => x.Key == SystemSettingKeyConstants.CONTACT_INFORMATION)
            ?.Value as IEnumerable<ContactInformationSetting>;

        return new ContactUsFeatureDTO(
            request.ContactUs.Title,
             request.ContactUs.Descrition,
             storeHours ?? [],
             contactInformation ?? []);
    }
}
