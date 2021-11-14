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

Now you can run a simple GraphQL query. We will get the first 30 students and in the linked data we will only take courses with "Calculus".

>The fields "CalculatedField" and "CalculatedField2" are calculated fields from the example in the repository. "CalculatedField" is a constant, so it will not be present in the SQL query.

```graphql
query {
  students(
    take: 30,
    orderBy: "enrollmentDate desc",
    where: [
      {
        "Path": "enrollmentDate", "Comparison": "LESS_THAN_OR_EQUAL", "Value": "2005-01-01"
      }
    ]
  ) {
    ID
    LastName
    CalculatedField
    CalculatedField2
    EnrollmentDate
    Enrollments(
      orderBy: "grade, course.title",
      where: [
        {
          Path: "Course.Title", Value: "Calculus"
        }
      ]
    ) {
      Grade
      Course {
        Title
        Test
      }
    }
  }
}
```

This query will be equivalent to the following expression

```sql
SELECT t."ID", t."LastName", t.c, t."EnrollmentDate", t0."Grade", t0.c, t0."Title", t0."EnrollmentID", t0."CourseID"
FROM (
    SELECT s."ID", s."LastName", (
        SELECT COUNT(*)::INT
        FROM "Enrollments" AS e
        WHERE (s."ID" = e."StudentID") AND (e."Grade" = 0)) AS c, s."EnrollmentDate"
    FROM "Students" AS s
    WHERE s."EnrollmentDate" <= TIMESTAMPTZ '2005-01-01 00:00:00Z'
    ORDER BY s."EnrollmentDate" DESC
    LIMIT @__p_1 OFFSET @__p_0
) AS t
LEFT JOIN (
    SELECT e0."Grade", FALSE AS c, c."Title", e0."EnrollmentID", c."CourseID", e0."StudentID"
    FROM "Enrollments" AS e0
    INNER JOIN "Courses" AS c ON e0."CourseID" = c."CourseID"
    WHERE c."Title" = 'Calculus'
) AS t0 ON t."ID" = t0."StudentID"
ORDER BY t."EnrollmentDate" DESC, t."ID", t0."Grade", t0."Title", t0."EnrollmentID"
```