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
                  .OrderBy(x => x.Node.NodeOrder)
                  .Select(x => new DocumentPage
                  {
                      DocumentName = x.DocumentName,
                      NodeID = x.NodeID,
                      Node = new DocumentNode
                      {
                          NodeID = x.NodeID,
                          RelativeUrl = x.Node.RelativeUrl,
                          ParentNodeID = x.Node.ParentNodeID,
                          ClassName = x.Node.ClassName,
                          NodeOrder = x.Node.NodeOrder
                      },
                      IsEnableNavigation = x.IsEnableNavigation,
                      OgImage = x.OgImage,
                      OgTitle = x.OgTitle,
                      OgDescription = x.OgDescription
                  });
              }, cache => cache.Dependencies(d => d.Nodes().NodeOrder()).Key("navigations"));

        var navNodeIds = navigations.Select(x => (int?)x.NodeID).ToArray();
        var children = navNodeIds.Length == 0
            ? []
            : await _pageRetrieveContext.GetPagesAsync<DocumentPage>(query =>
              {
                  query.Where(x => navNodeIds.Contains(x.Node.ParentNodeID) && x.Node.ClassName == "Pages.ProductCategory")
                  .OrderBy(x => x.Node.NodeOrder)
                  .Select(x => new DocumentPage
                  {
                      DocumentName = x.DocumentName,
                      NodeID = x.NodeID,
                      Node = new DocumentNode
                      {
                          NodeID = x.NodeID,
                          RelativeUrl = x.Node.RelativeUrl,
                          ParentNodeID = x.Node.ParentNodeID,
                          ClassName = x.Node.ClassName,
                          NodeOrder = x.Node.NodeOrder
                      },
                      OgImage = x.OgImage,
                      OgTitle = x.OgTitle,
                      OgDescription = x.OgDescription
                  });
              }, cache => cache.Dependencies(d => d.Nodes().NodeOrder()).Key("navigations_children"));

        var allPages = navigations.Concat(children).ToList();

        var imageGuids = allPages.SelectMany(x => x.OgImage).ToArray();
        var images = (await _mediaService.GetMediaItemsAsync(imageGuids)).ToDictionary(x => x.FileID, x => x.FileURL);

        var navItems = allPages.MappingTree()
        .Select(x => new NavItem(x, images));

        return navItems;
    }
}
