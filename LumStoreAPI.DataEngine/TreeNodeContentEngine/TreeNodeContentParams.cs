using System.Linq.Expressions;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.DataEngine.Models;
using LumStoreAPI.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.DataEngine.TreeNodeContentEngine
{
    internal partial class TreeNodeContent<T>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LumStoreContext _context;
        private IQueryable<T> _query;
        private bool _isIncludeRelativeUrl = false;
        private int _currentPage = -1;
        private int _pageSize = 0;
        private TreeNodePublished _treeNodePublished = TreeNodePublished.Published;
        private bool _checkAuthentication = true;
        private bool _hasPagination => _currentPage > 0 && _pageSize > 0;

        private Expression<Func<T, T>>? _selector = null;

        private IQueryable<DocumentLinkAliasModel> _relativeQuery
        {
            get
            {
                return _context.DocumentLinkedNodes
               .Join(_context.DocumentNodes,
               link => link.Ancestor,
               node => node.NodeID,
               (link, node) => new { link.Descendant, link.Depth, node.NodeAlias })
               .Select(x => new DocumentLinkAliasModel { Descendant = x.Descendant, Depth = x.Depth, NodeAlias = x.NodeAlias });

            }
        }

        private IQueryable<DocumentNodePathModel<T>> _relativeUrlQuery
        {
            get
            {
                var dataQuery = _query
                    .AsSplitQuery()
                  .GroupJoin(_relativeQuery,
                  node => node.NodeID,
                  path => path.Descendant,
                  (node, path) => new DocumentNodePathModel<T> { Node = node, Path = string.Join("/", path.OrderByDescending(x => x.Depth).Select(p => p.NodeAlias)) });

                return dataQuery;

            }
        }
    }
}
