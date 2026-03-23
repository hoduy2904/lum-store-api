using LumStoreAPI.Core.Interfaces.DocumentPages;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LumStoreAPI.DataEngine.TreeNodeContentEngine
{
    internal partial class TreeNodeContent<T>
    {
        public ITreeNodeContent<T> GetAncestor(int level)
        {
            level += 1;
            _query = _query
                .Where(x => _context.DocumentLinkedNodes.Any(l => l.Depth == level && l.Descendant == x.NodeID));

            return this;
        }

        public ITreeNodeContent<T> Where(Expression<Func<T, bool>> predicate)
        {
            _query = _query.Where(predicate);
            return this;
        }

        public IQueryable<T> AsQueryable()
        {
            return _query;
        }

        public ITreeNodeContent<T> GetAncestors()
        {
            _query = _query = _query
                .Where(x => _context.DocumentLinkedNodes.Any(l => l.Depth > 0 && l.Descendant == x.NodeID));
            return this;
        }

        public ITreeNodeContent<T> IncludeRelativeUrl(bool isInclude = true)
        {
            _isIncludeRelativeUrl = isInclude;
            return this;
        }

        public ITreeNodeContent<T> GetDescendants()
        {
            _query = _query
               .Where(x => _context.DocumentLinkedNodes.Any(l => l.Depth > 0 && l.Ancestor == x.NodeID));

            return this;
        }

        public ITreeNodeContent<T> GetDescendants(int level)
        {
            _query = _query
               .Where(x => _context.DocumentLinkedNodes.Any(l => l.Depth == level && l.Ancestor == x.NodeID));

            return this;
        }

        public ITreeNodeContent<T> IncludeQueryable(Func<IQueryable<T>, IQueryable<T>> expression)
        {
            _query = expression.Invoke(_query);
            return this;
        }

        public ITreeNodeContent<T> Published(TreeNodePublished treeNodePublished = TreeNodePublished.All)
        {
            _treeNodePublished = treeNodePublished;
            return this;
        }

        public ITreeNodeContent<T> Paged(int page, int pageSize)
        {
            _currentPage = page;
            _pageSize = pageSize;
            return this;
        }

        public ITreeNodeContent<T> RequiredAuthentication(bool isRequired = true)
        {
            _checkAuthentication = isRequired;
            return this;
        }

        public ITreeNodeContent<T> FindByNodeAlias(string nodeAlias)
        {
            if (nodeAlias.StartsWith("/"))
                nodeAlias = nodeAlias.TrimStart('/');

            _query = _query
                .Select(x => x.NodeID)
                .Join(_relativeQuery,
                node => node,
                r => r.Descendant,
                (node, r) => r)
                .GroupBy(x => x.Descendant)
                .Select(x => new { NodeID = x.Key, Path = string.Join("/", x.Select(s => s.NodeAlias)) })
                .Where(x => x.Path.Equals(nodeAlias))
                .Select(x => x.NodeID)
                .Join(_context.Set<T>(),
                nw => nw,
                r => r.NodeID,
                (nw, r) => r);


            return this;
        }

        public ITreeNodeContent<T> Roots()
        {
            _query = _query
                .Where(x => !_context.DocumentLinkedNodes.Any(n => n.Descendant == x.NodeID && n.Ancestor != x.NodeID));
            return this;
        }

        public ITreeNodeContent<T> Select(Expression<Func<T, T>> selector)
        {
            _selector = selector;
            return this;
        }
    }
}
