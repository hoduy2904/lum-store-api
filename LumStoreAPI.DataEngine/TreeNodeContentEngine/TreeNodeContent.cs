using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.DocumentPages;
using LumStoreAPI.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace LumStoreAPI.DataEngine.TreeNodeContentEngine
{
    internal partial class TreeNodeContent<T> : ITreeNodeContent<T> where T : DocumentPage
    {
        public TreeNodeContent(LumStoreContext context, IQueryable<T> query, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _query = query;
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerator<T> GetEnumerator()
        {
            LatestQueries();
            if (_isIncludeRelativeUrl)
            {
                return RelativeUrlFunc(_relativeUrlQuery);
            }
            return _query.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            LatestQueries();
            if (_isIncludeRelativeUrl)
            {
                return RelativeUrlFuncAsync(_relativeUrlQuery, cancellationToken);
            }
            return _query.AsAsyncEnumerable().GetAsyncEnumerator(cancellationToken);
        }

    }
}
