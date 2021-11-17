#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Example/Example.csproj", "Example/"]
COPY ["GraphQL.HotChocolate/GraphQL.HotChocolate.csproj", "GraphQL.HotChocolate/"]
COPY ["GraphQl.EfCore.Translate/GraphQl.EfCore.Translate.csproj", "GraphQl.EfCore.Translate/"]
COPY ["GraphQl.EfCore.Translate.HotChocolate/GraphQl.EfCore.Translate.HotChocolate.csproj", "GraphQl.EfCore.Translate.HotChocolate/"]
COPY ["Entity/Entity.csproj", "Entity/"]
COPY ["GraphQL.DotNet/GraphQL.DotNet.csproj", "GraphQL.DotNet/"]
COPY ["GraphQl.EfCore.Translate.DotNet/GraphQl.EfCore.Translate.DotNet.csproj", "GraphQl.EfCore.Translate.DotNet/"]
RUN dotnet restore "Example/Example.csproj"
COPY . .
WORKDIR "/src/Example"
RUN dotnet build "Example.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Example.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Example.dll"]