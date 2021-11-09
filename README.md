# GraphQl.EfCore.Translate

[![NuGet version](https://badge.fury.io/nu/GraphQl.EfCore.Translate.svg)](https://badge.fury.io/nu/GraphQl.EfCore.Translate)

The package adds extensions to EntityFrameworkCore that allow you to transform a GraphQL query into an EntityFrameworkCore query. The project solves the problem of a large amount of data dumping, filtering related data and adding calculated fields.

## Start

### Install

Install the package

```powershell
Install-Package GraphQl.EfCore.Translate
```

Connect services in Startup.cs file

```C#
public void ConfigureServices(IServiceCollection services)
{
    ...

    services.AddGraphQLTranslate();

    ...
}
```

Set the converter in your schema to "DefaultNameConverter", for example

```C#
public GraphQLSchema(IServiceProvider services) : base(services)
{
  Query = services.GetRequiredService<MainQuery>();
  NameConverter = new DefaultNameConverter();
}
```

### Usage

```C#
using GraphQl.EfCore.Translate;

...

Field<ListGraphType<StudentObject>, List<Student>>("Students")
  .Argument<IntGraphType>("Take")
  .Argument<IntGraphType>("Skip")
  .Argument<StringGraphType>("OrderBy")
  .Argument<ListGraphType<WhereExpressionGraph>>("Where")
  .Resolve().WithScope().WithService<SchoolContext>()
  .ResolveAsync((context, dbContext) =>
  {
    var query = dbContext.Students
                              .GraphQlWhere(context)
                              .GraphQlOrder(context)
                              .GraphQlPagination(context)
                              .GraphQlSelect(context);

    return query.ToListAsync();
  });
```

Now you can run a simple GraphQL query. We will get the first 30 students and in the linked data we will only take courses with an "A" or "B" grade.

```graphql
query {
  Students(
    Skip: 0,
    Take: 30,
    OrderBy: "EnrollmentDate desc",
    Where: [
      {
        Path: "EnrollmentDate", Comparison: "lessThanOrEqual", Value: "2005-01-01"
      }
    ]
  ) {
    ID
    LastName
    EnrollmentDate
    Enrollments(
      OrderBy: "Grade",
      Where: [
        {
          Path: "Grade", Comparison: "in", Value: ["A", "B"]
        }
      ]
    ) {
      Grade
      Course {
        Title
      }
    }
  }
}
```

This query will be equivalent to the following expression

```C#
var query = dbContext.Students
  .Where(x => x.EnrollmentDate <= DateTime.Parse("01.01.2005"))
  .OrderByDescending(x => x.EnrollmentDate)
  .Skip(0)
  .Take(30)
  .Select(x => new User {
    ID = x.ID,
    LastName = x.LastName,
    EnrollmentDate = x.EnrollmentDate,
    Enrollments = x.Enrollments
                .Where(c => (new string[] {"A", "B"}).Contains(c.Grade))
                .OrderBy(x => x.Grade)
                .Select(c => new Enrollment {
                  Grade = c.Grade,
                  Course = new Course {
					          Title = c.Course.Title
				          }
                })
  });
```
