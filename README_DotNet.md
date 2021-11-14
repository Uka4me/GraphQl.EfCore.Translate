# GraphQl.EfCore.Translate.DotNet

[![NuGet version](https://badge.fury.io/nu/GraphQl.EfCore.Translate.DotNet.svg)](https://badge.fury.io/nu/GraphQl.EfCore.Translate.DotNet)

## Documentation
[Wiki](https://github.com/Uka4me/GraphQl.EfCore.Translate/wiki)

## Start

### Install

Install the package

```powershell
Install-Package GraphQl.EfCore.Translate.DotNet
```

Connect services in Startup.cs file

```C#
using GraphQl.EfCore.Translate.DotNet;

public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddEFCoreGraphQLDotNet();
    /*
      Or

      services.AddSingleton<GraphQl.EfCore.Translate.DotNet.StringComparisonGraph>();
      services.AddSingleton<GraphQl.EfCore.Translate.DotNet.WhereExpressionGraph>();
      services.AddSingleton<GraphQl.EfCore.Translate.DotNet.ComparisonGraph>();
      services.AddSingleton<GraphQl.EfCore.Translate.DotNet.ConnectorGraph>();
    */
    ...
}
```

### Usage

```C#
using GraphQl.EfCore.Translate.DotNet;

...

Field<ListGraphType<StudentObject>, List<Student>>("students")
  .Argument<IntGraphType>("take")
  .Argument<IntGraphType>("skip")
  .Argument<StringGraphType>("orderBy")
  .Argument<ListGraphType<WhereExpressionGraph>>("where")
  .Resolve().WithScope().WithService<SchoolContext>()
  .ResolveAsync((context, dbContext) =>
  {
    var query = dbContext.Students.GraphQl(context);
    // Or
    /* var query = dbContext.Students
                              .GraphQlWhere(context)
                              .GraphQlOrder(context)
                              .GraphQlPagination(context)
                              .GraphQlSelect(context); */

    return query.ToListAsync();
  });
```

Now you can run a simple GraphQL query. We will get the first 30 students and in the linked data we will only take courses with an "A" or "B" grade.

```graphql
query {
  students(
    skip: 0,
    take: 30,
    orderBy: "enrollmentDate desc",
    where: [
      {
        path: "enrollmentDate", comparison: "lessThanOrEqual", value: "2005-01-01"
      }
    ]
  ) {
    iD
    lastName
    enrollmentDate
    enrollments(
      orderBy: "grade",
      where: [
        {
          path: "grade", comparison: "in", value: ["A", "B"]
        }
      ]
    ) {
      grade
      course {
        title
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
  .Select(x => new Student {
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