using Infrastructure.Common.Models;
using MongoDB.Driver;

namespace Infrastructure.Extensions;

public static class MongoCollectExtensions
{
     public static Task<PagedList<TDestination>> PagingtedListAsync<TDestination>(
          this IMongoCollection<TDestination> collection,
          FilterDefinition<TDestination> filter,
          int pageIndex,
          int pageSize) where TDestination : class
     => PagedList<TDestination>.ToPagedList(collection, filter, pageIndex, pageSize);
}