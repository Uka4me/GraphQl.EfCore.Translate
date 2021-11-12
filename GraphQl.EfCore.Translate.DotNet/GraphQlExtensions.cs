using GraphQL;
using GraphQL.Language.AST;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQl.EfCore.Translate.DotNet;

public static class GraphQlExtensions
{
    //TODO: remove in v16 to drop support for quoted enum values
    public static IValue TryToEnumValue(this IValue value)
    {
        if (value is StringValue stringValue)
        {
            return new EnumValue(stringValue.Value);
        }

        return value;
    }

    #region ExecuteWithErrorCheck

    public static async Task<ExecutionResult> ExecuteWithErrorCheck(
        this IDocumentExecuter executer,
        ExecutionOptions options)
    {
        var executionResult = await executer.ExecuteAsync(options);

        var errors = executionResult.Errors;
        if (errors != null && errors.Count > 0)
        {
            if (errors.Count == 1)
            {
                throw errors.First();
            }

            throw new AggregateException(errors);
        }

        return executionResult;
    }

    #endregion
}