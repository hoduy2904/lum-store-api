using LumStoreAPI.DataEngine.Models;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.DataEngine.TreeNodeContentEngine
{
    internal partial class TreeNodeContent<T>
    {
        private Func<IQueryable<DocumentNodePathModel<T>>, IEnumerator<T>> RelativeUrlFunc = (documentNodePath)
            => documentNodePath.AsEnumerable().Select(x =>
            {
                x.Node.Node.RelativeUrl = x.Path;
                return x.Node;
            }).GetEnumerator();

        private Func<IQueryable<DocumentNodePathModel<T>>, CancellationToken, IAsyncEnumerator<T>> RelativeUrlFuncAsync = (documentNodePath, cancellationToken)
            => documentNodePath.AsAsyncEnumerable().Select(x =>
            {
                x.Node.Node.RelativeUrl = x.Path;
                return x.Node;
            }).GetAsyncEnumerator(cancellationToken);

        private void LatestQueries()
        {
            if (_hasPagination)
            {
                _query = _query
                    .Skip((_currentPage - 1) * _pageSize)
                    .Take(_pageSize);
            }
            if (_treeNodePublished != Core.Models.Enums.TreeNodePublished.All)
            {
                var now = DateTime.UtcNow;
                if (_treeNodePublished == Core.Models.Enums.TreeNodePublished.Published)
                {
                    _query = _query.Where(x =>
                    (x.PublishedFrom == null || x.PublishedFrom <= now)
                    && x.PublishedTo == null || x.PublishedTo >= now);
                }
                else
                {
                    _query = _query.Where(x =>
                    (x.PublishedFrom != null && x.PublishedFrom > now)
                    || (x.PublishedTo != null && x.PublishedTo < now));
                }
            }
            if (_checkAuthentication)
            {
                var isAuthentication = _httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
                _query = _query.Where(x => !x.RequireAuthentication || isAuthentication);
            }
            if (_selector != null)
            {
                _query = _query.Select(_selector);
            }
        }

    }
}
