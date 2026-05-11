using System;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Libraries.Extensions;

namespace LumStoreAPI.Application.Services;

public class SiteService(
    IPageRetrieveContext pageRetrieveContext,
    IMediaService mediaService
) : ISiteService
{
    private readonly IPageRetrieveContext _pageRetrieveContext = pageRetrieveContext;
    private readonly IMediaService _mediaService = mediaService;
    public async Task<IEnumerable<NavItem>> GetNavigationsAsync()
    {
        var navigations = await _pageRetrieveContext.GetPagesAsync<DocumentPage>(query =>
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
              }, cache => cache.Dependencies(d => d.Nodes().NodeOrder()).Key("navigations"));

        var imageGuids = navigations.SelectMany(x => x.OgImage).ToArray();
        var images = (await _mediaService.GetMediaItemsAsync(imageGuids)).ToDictionary(x => x.FileID, x => x.FileURL);

        var navItems = navigations.MappingTree()
        .Select(x => new NavItem(x, images));

        return navItems;
    }
}
