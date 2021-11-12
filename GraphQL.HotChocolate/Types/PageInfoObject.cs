using Entity.Classes;
using Entity.Models;
using GraphQl.EfCore.Translate.HotChocolate;

namespace GraphQL.HotChocolate.Types
{
    public class PageInfoObject<T, U> : ObjectType<PageInfo<U>> where T : IType
    {
        protected override void Configure(IObjectTypeDescriptor<PageInfo<U>> descriptor)
        {
            descriptor.Field(t => t.Total).Type<IntType>();
            descriptor.Field(t => t.CurrentPage).Type<IntType>();
            descriptor.Field(t => t.Data).Type<ListType<T>>();
            descriptor.Field("test").Type<StringType>().Resolve(x => "Привет");
            /*descriptor.Field(t => t.Enrollments).Type<ListType<EnrollmentObject>>()
                .Argument("take", a => a.Type<IntType>())
                .Argument("skip", a => a.Type<IntType>())
                .Argument("orderBy", a => a.Type<StringType>())
                .Argument("where", a => a.Type<ListType<WhereExpressionGraph>>());*/
        }
    }
}
