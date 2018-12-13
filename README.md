# EF Core - Pagination `IQueryable<T>` Extensions

Are you bored of using the `Repository<T>` and `GenericRepository<T>` just to get a paged result from query against a `DbSet<T>`? Good! If so, then this is the only extension class you will ever need. EfQueryableExtensions introduces a `QueryPage()` extension method for `IQueryable<T>` which allows you to practically query a database with Entity Framework Core and retrieve a paged result.

## Example usage

The example below assumes you have installed Entity Framework Core and that you are querying against a DB table `dbo.Users` which is mapped with Entity Framework Core in your source.

Basic example without sort:

```csharp

  // select a page, size, filter it, include some related entites and project to a custom model
  // the total is calculated if needed, it takes in filtering and includes
  var (page, countTotal) = users.QueryPage(
    page: 2,
    size: 10,
    filter: x => x.Id < 10,
    includes: u => u.Include(x => x.Role),
    project: x => new
    {
      Id = x.Id,
        Email = x.Email,
        RoleName = x.Role.Name
    });

  var result = page.ToList();
  var total = countTotal();
```

Advanced example with sort:

```csharp

  // select a page, size, filter it, include some related entites, sort and project to a custom model
  // the total is calculated if needed, it takes in sort, filtering and includes
  var sort = new KeyValuePair<Expression<Func<User, object>>, EfQueryableExtensions.SortOrder>(
      x => x.Id,
      EfQueryableExtensions.SortOrder.Descending);

  var (page, countTotal) = users.QueryPage(
    page: 2,
    size: 10,
    filter: x => x.Id < 10,
    includes: u => u.Include(x => x.Role),
    project: x => new
    {
      Id = x.Id,
        Email = x.Email,
        RoleName = x.Role.Name
    },
    sort);

  var result = page.ToList();
  var total = countTotal();
```

## Dependencies

The `IQueryable<T>` extensions class depends on `Microsoft.EntityFrameworkCore (2.2.0)`.

## Project settings

The dotnet standard is: `2.0` <br/>
The .NET Core SDK is: `2.1.500`
