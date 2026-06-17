using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Core.Models.Systems.SettingKeys;
using LumStoreAPI.Libraries.Extensions;
using LumStoreAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.ClientControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LayoutController(
        ISettingKeyValueService settingKeyValueService,
        IPageRetrieveContext pageRetrieveContext,
        ISiteService siteService
    ) : ControllerBase
    {
        private readonly ISettingKeyValueService _settingKeyValueService = settingKeyValueService;
        private readonly IPageRetrieveContext _pageRetrieveContext = pageRetrieveContext;
        private readonly ISiteService _siteService = siteService;
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

            var navigations = await _siteService.GetNavigationsAsync();

            var footerColumns = generalSettings?.FooterColumn > 0 ? (await _pageRetrieveContext.GetPagesAsync<DocumentPage>(query =>
            {
                var classNameFilters = new string[] { LinkListItem.CLASS_NAME, "CMS.Folder" };
                query.GetDescendants(generalSettings.FooterColumn)
                .Where(x => classNameFilters.Contains(x.Node.ClassName));
            }, cache => cache.Dependencies(d => d.Children(generalSettings.FooterColumn).NodeOrder()).Key("footercolumns")))
            .MappingTree()
            .Select(x => NavItem.From(x, [])) : Enumerable.Empty<NavItem>();

            return Ok(APIResponse<SiteConfigViewModel>.Success(new SiteConfigViewModel(layoutSettings, keySettings, footerColumns, navigations)));
        }

        [HttpGet("sitemap")]
        public async Task<IActionResult> GetSitemap()
        {
           var pages = await _pageRetrieveContext.GetPagesAsync<DocumentPage>(query =>
            {
                query.Where(x => x.Node.ClassName.StartsWith("Pages"))
                    .Select(x=> new DocumentPage
                    {
                        DocumentName = x.DocumentName,
                        UpdatedAt =  x.UpdatedAt,
                        CreatedAt =  x.CreatedAt,
                        Node = new DocumentNode
                        {
                            RelativeUrl =  x.Node.RelativeUrl
                        }
                    });
            },cache=>cache.Dependencies(d=>d.Nodes()).Key("sitemap").Expiration(-1));

            var sitemaps = pages.Select(x => new Sitemap
            {
                LastMod = x.UpdatedAt.ToUniversalTime().DateTime,
                Loc = x.Node.RelativeUrl
            });
            
            return Ok(APIResponse<IEnumerable<Sitemap>>.Success(sitemaps,["success"]));
        }
    }
}
