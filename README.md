# GraphQl.EfCore.Translate

The package adds extensions to EntityFrameworkCore that allow you to transform a GraphQL query into an EntityFrameworkCore query. The project solves the problem of a large amount of data dumping, filtering related data and adding calculated fields. The library is designed to work with [GraphQL](https://github.com/graphql-dotnet/graphql-dotnet) and [HotChocolate](https://github.com/ChilliCream/hotchocolate) projects.

The project is split into two libraries:
- `GraphQl.EfCore.Translate.DotNet` [![NuGet version](https://badge.fury.io/nu/GraphQl.EfCore.Translate.DotNet.svg)](https://badge.fury.io/nu/GraphQl.EfCore.Translate.DotNet)
- `GraphQl.EfCore.Translate.HotChocolate` [![NuGet version](https://badge.fury.io/nu/GraphQl.EfCore.Translate.HotChocolate.svg)](https://badge.fury.io/nu/GraphQl.EfCore.Translate.HotChocolate)

## Documentation
[Wiki](https://github.com/Uka4me/GraphQl.EfCore.Translate/wiki)

## Start

- [Start for GraphQl.EfCore.Translate.DotNet](/README_DotNet.md)
- [Start for GraphQl.EfCore.Translate.HotChocolate](/README_HotChocolate.md)

## Dependencies versions

### 3.0.0
- GraphQl.EfCore.Translate.DotNet
  + net6.0
  + GraphQL (>= 4.6.1)

- GraphQl.EfCore.Translate.HotChocolate
  + net6.0
  + HotChocolate (>= 12.2.1)