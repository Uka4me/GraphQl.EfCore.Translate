# GraphQl.EfCore.Translate

[![NuGet version](https://badge.fury.io/nu/GraphQl.EfCore.Translate.svg)](https://www.nuget.org/packages/GraphQl.EfCore.Translate/)

The package adds extensions to EntityFrameworkCore that allow you to transform a GraphQL query into an EntityFrameworkCore query. The project solves the problem of a large amount of data dumping, filtering related data and adding calculated fields.

## Start

### Install

```powershell
Install-Package GraphQl.EfCore.Translate
```

### A simple example

```C#
using GraphQl.EfCore.Translate;

...

Field<ListGraphType<UserObject>, List<User>>("Users")
  .Argument<IntGraphType>("Take")
  .Argument<IntGraphType>("Skip")
  .Argument<StringGraphType>("OrderBy")
  .Argument<ListGraphType<WhereExpressionGraph>>("Where")
  .Resolve().WithScope().WithService<DBContext>()
  .ResolveAsync((context, dbContext) =>
  {
    var query = dbContext.Users
                              .GraphQlWhere(context)
                              .GraphQlOrder(context)
                              .GraphQlPagination(context)
                              .GraphQlSelect(context);

    return query.ToListAsync();
  });
```

Now you can run a simple GraphQL query. We will get the first 30 user records and in the related data we will take only those cities in which the population is more than 1000 people.

```graphql
query {
  Users(
    Skip: 0,
    Take: 30,
    OrderBy: "CreatedAt desc",
    Where: [
      {
        Path: "Name", Comparison: "like", Value: "%John%" 
      }
    ]
  ) {
    Id
    Name
    Email
    Cities(
      OrderBy: "Population desc",
      Where: [
        {
          Path: "Population", Comparison: "greaterThan", Value: "1000" 
        }
      ]
    ) {
      Title
      Population
    }
  }
}
```

This query will be equivalent to the following expression

```C#
var query = dbContext.Users
  .Where(x => EF.Functions.Like(x.Name, "%John%"))
  .OrderByDescending(x => x.CreatedAt)
  .Skip(0)
  .Take(30)
  .Select(x => new User {
    Id = x.Id,
    Name = x.Name,
    Email = x.Email,
    Cities = x.Cities
                .Where(c => c.Population > 1000)
                .OrderByDescending(x => x.Population)
                .Select(c => new City {
                  Title = c.Title,
                  Population = c.Population
                })
  });
```
