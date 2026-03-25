using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Models.Enums;
using System.Linq.Expressions;

namespace LumStoreAPI.Core.Interfaces.DocumentPages
{
    public interface ITreeNodeContent<T> : IEnumerable<T>, IAsyncEnumerable<T> where T : DocumentPage
    {
        /// <summary>
        /// Level must greater than or equals 0
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        ITreeNodeContent<T> GetAncestor(int nodeId, int level);
        /// <summary>
        /// Convert to Entity framework prodivder
        /// Warning, some features of ITreeNodeContent will be lost
        /// </summary>
        /// <returns></returns>
        IQueryable<T> AsQueryable();
        ITreeNodeContent<T> Where(Expression<Func<T, bool>> predicate);
        /// <summary>
        /// Get all parents of current nodes
        /// </summary>
        /// <returns></returns>
        ITreeNodeContent<T> GetAncestors(int nodeId);
        /// <summary>
        /// Get all children nodes of current nodes
        /// </summary>
        /// <returns></returns>
        ITreeNodeContent<T> GetDescendants(int parentNodeId);
        ITreeNodeContent<T> GetDescendants(int parentNodeId, int level);
        /// <summary>
        /// Included relative url
        /// </summary>
        /// <param name="isInclude"></param>
        /// <returns></returns>
        ITreeNodeContent<T> IncludeRelativeUrl(bool isInclude = true);
        /// <summary>
        /// Please use order by before pagination
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        ITreeNodeContent<T> Paged(int page, int pageSize);
        /// <summary>
        /// Using Entity framework's queries
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        ITreeNodeContent<T> IncludeQueryable(Func<IQueryable<T>, IQueryable<T>> expression);
        ITreeNodeContent<T> Published(TreeNodePublished treeNodePublished = TreeNodePublished.All);
        ITreeNodeContent<T> FindByNodeAlias(string nodeAlias);
        ITreeNodeContent<T> Roots();

        ITreeNodeContent<T> Select(Expression<Func<T, T>> selector);
    }
}
