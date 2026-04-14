using LumStoreAPI.Core.Interfaces.DocumentPages;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LumStoreAPI.DataEngine.TreeNodeContentEngine
{
    internal partial class TreeNodeContent<T>
    {
        public ITreeNodeContent<T> GetAncestor(int nodeId, int level)
        {
            level += 1;
            _query = _query
           .Join(_context.DocumentLinkedNodes,
           n => n.NodeID,
           ln => ln.Descendant,
           (n, ln) => new { n, ln }
           )
              .Where(x => x.ln.Descendant == nodeId && x.ln.Depth == level)
              .Select(x => x.n);
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

        public ITreeNodeContent<T> GetAncestors(int nodeId)
        {
            _query = _query
           .Join(_context.DocumentLinkedNodes,
           n => n.NodeID,
           ln => ln.Descendant,
           (n, ln) => new { n, ln }
           )
              .Where(x => x.ln.Descendant == nodeId && x.ln.Depth > 0)
              .Select(x => x.n);
            return this;
        }

        public ITreeNodeContent<T> IncludeRelativeUrl(bool isInclude = true)
        {
            _isIncludeRelativeUrl = isInclude;
            return this;
        }

        public ITreeNodeContent<T> GetDescendants(int parentNodeId)
        {
            _query = _query
            .Join(_context.DocumentLinkedNodes,
            n => n.NodeID,
            ln => ln.Descendant,
            (n, ln) => new { n, ln }
            )
               .Where(x => x.ln.Ancestor == parentNodeId && x.ln.Depth > 0)
               .Select(x => x.n);

            return this;
        }

        public ITreeNodeContent<T> GetDescendants(int parentNodeId, int level)
        {
            _query = _query
            .Join(_context.DocumentLinkedNodes,
            n => n.NodeID,
            ln => ln.Descendant,
            (n, ln) => new { n, ln }
            )
               .Where(x => x.ln.Ancestor == parentNodeId && x.ln.Depth == level)
               .Select(x => x.n);

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

        public ITreeNodeContent<T> OnlyPages()
        {
            _query = _query.Where(x => x.Node.ClassName.StartsWith("Pages."));
            return this;
        }

        public ITreeNodeContent<T> Select(Expression<Func<T, T>> selector)
        {
            _selector = selector;
            return this;
        }
    }
}
