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

        /// <summary>
        /// Only get page can show on the client
        /// </summary>
        /// <returns></returns>
        ITreeNodeContent<T> OnlyPages();

        /// <summary>
        /// Filters to the <em>direct</em> children of <paramref name="parentNodeId"/> using
        /// <c>DocumentNode.ParentNodeID</c> (the FK column), not the closure table.
        /// <para>
        /// Prefer this over <see cref="GetDescendants(int,int)"/> with depth=1 when
        /// <c>DocumentLinkedNode</c> rows may not be fully populated for every node.
        /// </para>
        /// </summary>
        /// <param name="parentNodeId">NodeID of the parent whose immediate children are wanted.</param>
        ITreeNodeContent<T> GetChildren(int parentNodeId);

        /// <summary>
        /// Applies an ascending ORDER BY on the underlying <see cref="IQueryable{T}"/>.
        /// <para>
        /// Must be called <b>before</b> <see cref="Paged"/> so that pagination skips/takes
        /// are applied on an already-ordered set.
        /// </para>
        /// <para>
        /// The ordering is preserved through subsequent <c>Where</c>, published-filter,
        /// and <c>Select</c> operations because all are appended to the same expression tree
        /// and translated to a single SQL query.
        /// </para>
        /// </summary>
        /// <typeparam name="TKey">Type of the sort key.</typeparam>
        /// <param name="keySelector">Expression selecting the sort key from the page entity.</param>
        ITreeNodeContent<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
    }
}
