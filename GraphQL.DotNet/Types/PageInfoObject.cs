using Entity.Classes;
using Entity.Models;
using GraphQL.DotNet.Queries;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphQL.DotNet.Types
{
    public sealed class PageInfoObject<T, U> : ObjectGraphType<PageInfo<U>> where T : IGraphType
    {
        public PageInfoObject()
        {
            Name = nameof(PageInfo<U>);

            Field(m => m.Total);
            Field(
                name: "Data",
                type: typeof(ListGraphType<T>),
                resolve: m => m.Source.Data);
            Field(m => m.CurrentPage);
            Field<StringGraphType>("Test", resolve: m => "Привет");
        }

    }
}
