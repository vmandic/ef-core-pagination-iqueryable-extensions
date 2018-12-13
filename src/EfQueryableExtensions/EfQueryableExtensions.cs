using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace EfQueryableExtensions
{
  public enum SortOrder
  {
    Ascending = 0,
    Descending = 1
  }

  public static class EfQueryableExtensions
  {
    const int PAGE_SIZE_DEFAULT = 12;

    public static(IQueryable<T> pagedQuery, Func<int> countTotal) QueryPage<T>(
      this IQueryable<T> baseQuery,
      int? page = 1,
      int? size = PAGE_SIZE_DEFAULT,
      Expression<Func<T, bool>> filter = null,
      Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null
    ) where T : class
    {
      if (!page.HasValue)
        throw new ArgumentNullException(nameof(page));

      if (!size.HasValue)
        throw new ArgumentNullException(nameof(size));

      var filteredAndIncludedQuery = FilterAndInclude(baseQuery, filter, includes);

      var pagedFilteredAndIncludedQuery =
        Paginate(filteredAndIncludedQuery, size.Value, (page.Value - 1) * size.Value);

      int countTotal() => filteredAndIncludedQuery.Count();

      return (pagedQuery: pagedFilteredAndIncludedQuery, countTotal);
    }

    public static(IQueryable<ProjectT> pagedQuery, Func<int> countTotal) QueryPage<T, ProjectT>(
      this IQueryable<T> baseQuery,
      int? page = 1,
      int? size = PAGE_SIZE_DEFAULT,
      Expression<Func<T, bool>> filter = null,
      Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null,
      Expression<Func<T, ProjectT>> project = null
    ) where T : class
    {
      var (pagedQuery, countTotal) =
      QueryPage(baseQuery, page, size, filter, includes);

      return (pagedQuery.Select(project), countTotal);
    }

    public static(IQueryable<ProjectT> pagedQuery, Func<int> countTotal) QueryPage<T, ProjectT>(
      this IQueryable<T> baseQuery,
      int? page = 1,
      int? size = PAGE_SIZE_DEFAULT,
      Expression<Func<T, bool>> filter = null,
      Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null,
      Expression<Func<T, ProjectT>> project = null,
      params KeyValuePair<Expression<Func<T, object>>, SortOrder >[] orderBys
    ) where T : class
    {
      var sortedQuery = Sort(baseQuery, orderBys);

      var (sortedPagedFilteredAndIncludedQuery, countTotal) =
      QueryPage(sortedQuery, page, size, filter, includes);

      return (pagedQuery: sortedPagedFilteredAndIncludedQuery.Select(project), countTotal);
    }

    private static IQueryable<T> Paginate<T>(IQueryable<T> baseQuery, int size, int skipCount) =>
      skipCount == 0 ?
      baseQuery.Take(size) :
      baseQuery.Skip(skipCount).Take(size);

    private static IQueryable<T> FilterAndInclude<T>(
      IQueryable<T> baseQuery,
      Expression<Func<T, bool>> filter,
      Func<IQueryable<T>, IIncludableQueryable<T, object>> includes) where T : class
    {
      var queryIncluded = Include(baseQuery, includes);

      return filter != null ?
        queryIncluded.Where(filter) :
        queryIncluded;
    }

    private static IQueryable<T> Sort<T>(
      IQueryable<T> baseQuery,
      KeyValuePair<Expression<Func<T, object>>, SortOrder >[] orderBys)
    {
      if (orderBys != null && orderBys.Length > 0)
      {
        foreach (var ordering in orderBys)
        {
          baseQuery = ordering.Value == SortOrder.Ascending ?
            baseQuery.OrderBy(ordering.Key) :
            baseQuery.OrderByDescending(ordering.Key);
        }
      }

      return baseQuery;
    }

    private static IQueryable<T> Include<T>(
        IQueryable<T> baseQuery,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> includes) =>
      includes?.Invoke(baseQuery) ?? baseQuery;
  }
}
