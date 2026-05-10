using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Helpers;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Systems.SettingKeys;
using LumStoreAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.ClientControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LayoutController(
        ISettingKeyValueService settingKeyValueService,
        IPageRetrieveContext pageRetrieveContext
    ) : ControllerBase
    {
        private readonly ISettingKeyValueService _settingKeyValueService = settingKeyValueService;
        private readonly IPageRetrieveContext _pageRetrieveContext = pageRetrieveContext;
        [HttpGet("site-config")]
        public async Task<IActionResult> GetSiteConfig()
        {
            var layoutSettings = await _settingKeyValueService
             .GetSystemSettingsAsync(
                 SystemSettingKeyConstants.MARQUEE_LIST,
                 SystemSettingKeyConstants.FOOTER_NAVIGATIONS,
                 SystemSettingKeyConstants.GENERAL_SETTINGS);

            var generalSettings = layoutSettings
                .FirstOrDefault(x => x.Key.Equals(SystemSettingKeyConstants.GENERAL_SETTINGS))?
                .Value as GeneralSettings;

            var keySettings = await _settingKeyValueService.GetSettingsAsync(
                "INSTAGRAM_URL", "TIKTOK_URL"
            );

            var navigations = (await _pageRetrieveContext.GetPagesAsync<DocumentPage>(query =>
            {
                query.Where(x => x.IsEnableNavigation)
                .Select(x => new DocumentPage
                {
                    DocumentName = x.DocumentName,
                    NodeID = x.NodeID,
                    Node = new DocumentNode
                    {
                        NodeID = x.NodeID,
                        RelativeUrl = x.Node.RelativeUrl,
                        ParentNodeID = x.Node.ParentNodeID,
                        ClassName = x.Node.ClassName
                    },
                    IsEnableNavigation = x.IsEnableNavigation,
                    OgImage = x.OgImage,
                    OgTitle = x.OgTitle,
                    OgDescription = x.OgDescription
                });
            }, cache => cache.Dependencies(d => d.Nodes().NodeOrder()).Key("navigations")))
            .DocumentClientGetLinkeds();

            var footerColumns = generalSettings?.FooterColumn > 0 ? (await _pageRetrieveContext.GetPagesAsync<DocumentPage>(query =>
            {
                var classNameFilters = new string[] { LinkListItem.CLASS_NAME, "CMS.Folder" };
                query.GetDescendants(generalSettings.FooterColumn)
                .Where(x => classNameFilters.Contains(x.Node.ClassName))
                .Select(x => new DocumentPage
                {
                    DocumentName = x.DocumentName,
                    NodeID = x.NodeID,
                    Node = new DocumentNode
                    {
                        NodeID = x.Node.NodeID,
                        ParentNodeID = x.Node.ParentNodeID,
                        RelativeUrl = x.Node.RelativeUrl,
                        ClassName = x.Node.ClassName
                    }
                });
            }, cache => cache.Dependencies(d => d.Children(generalSettings.FooterColumn).NodeOrder()).Key("footercolumns")))
            .DocumentClientGetLinkeds() : Enumerable.Empty<DocumentClientGetLinkedDTO>();

            return Ok(APIResponse<SiteConfigViewModel>.Success(new SiteConfigViewModel(layoutSettings, keySettings, footerColumns, navigations)));
        }
    }
}
