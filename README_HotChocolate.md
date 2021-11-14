# GraphQl.EfCore.Translate.HotChocolate

[![NuGet version](https://badge.fury.io/nu/GraphQl.EfCore.Translate.HotChocolate.svg)](https://badge.fury.io/nu/GraphQl.EfCore.Translate.HotChocolate)

## Documentation
[Wiki](https://github.com/Uka4me/GraphQl.EfCore.Translate/wiki)

## Start

### Install

Install the package

```powershell
Install-Package GraphQl.EfCore.Translate.HotChocolate
```

Connect services in Startup.cs file

```C#
using GraphQl.EfCore.Translate.HotChocolate;

public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddEFCoreGraphQLHotChocolate();
    /*
      Or

      services.AddSingleton<GraphQl.EfCore.Translate.HotChocolate.StringComparisonGraph>();
      services.AddSingleton<GraphQl.EfCore.Translate.HotChocolate.WhereExpressionGraph>();
      services.AddSingleton<GraphQl.EfCore.Translate.HotChocolate.ComparisonGraph>();
      services.AddSingleton<GraphQl.EfCore.Translate.HotChocolate.ConnectorGraph>();
    */
    ...
}
```

### Usage

```C#
using GraphQl.EfCore.Translate.HotChocolate;

...

[UseDbContext(typeof(SchoolContext))]
public List<Student> GetStudents([ScopedService] SchoolContext dbContext, IResolverContext context, int take = 0, int skip = 0, string orderBy = "", List<WhereExpression>? where = default) {
    var query = dbContext.Students.GraphQl(context);
    // Or
    /* var query = dbContext.Students
        .GraphQlWhere(context)
        .GraphQlOrder(context)
        .GraphQlPagination(context)
        .GraphQlSelect(context); */

    return query.ToList();
}
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