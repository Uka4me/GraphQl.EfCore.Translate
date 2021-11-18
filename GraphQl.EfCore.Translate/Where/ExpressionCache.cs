using System.Linq.Expressions;

static class ExpressionCache
{
    public static ConstantExpression NegativeOne = Expression.Constant(-1);
    public static ConstantExpression False = Expression.Constant(false, typeof(bool));
    public static ConstantExpression Null = Expression.Constant(null, typeof(object));
    /*public static ConstantExpression EfFunction = Expression.Constant(EF.Functions);*/
    public static ParameterExpression StringParam = Expression.Parameter(typeof(string));
}