﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using GraphQl.EfCore.Translate.Converters;
using GraphQl.EfCore.Translate.Where.Graphs;

namespace GraphQl.EfCore.Translate
{
    public static class ExpressionBuilderWhere<T>
    {
        const string LIST_PROPERTY_PATTERN = @"\[(.*)\]";

        #region Conditional List

        /// <summary>
        /// Build a predicate for a supplied list of where's (Grouped or not)
        /// </summary>
        public static Expression<Func<T, bool>> BuildPredicate(IEnumerable<WhereExpression> wheres)
        {
            var expressionBody = MakePredicateBody(wheres);
            var param = PropertyCache<T>.SourceParameter;

            return Expression.Lambda<Func<T, bool>>(expressionBody, param);
        }

        /// <summary>
        /// Makes the predicate body from the supplied parameter and list of where expressions
        /// </summary>
        private static Expression MakePredicateBody(IEnumerable<WhereExpression> wheres)
        {
            Expression? mainExpression = null;
            WhereExpression previousWhere = new();

            // Iterate over wheres
            foreach (var where in wheres)
            {
                Expression nextExpression;

                // If there are grouped expressions
                if (@where.GroupedExpressions?.Count > 0)
                {
                    // Recurse with new set of expression
                    nextExpression = MakePredicateBody(where.GroupedExpressions);

                    // If the whole group is to be negated
                    if (where.Negate)
                    {
                        // Negate it
                        nextExpression = NegateExpression(nextExpression);
                    }
                }
                // Otherwise handle single expressions
                else
                {
                    // Get the predicate body for the single expression
                    nextExpression = MakePredicateBody(where.Path, where.Comparison, where.Value?.ToArray(), where.Negate, where.Case);
                }

                // If this is the first where processed
                if (mainExpression is null)
                {
                    // Assign to main expression
                    mainExpression = nextExpression;
                }
                else
                {
                    // Otherwise combine expression by specified connector or default (AND) if not provided
                    mainExpression = CombineExpressions(previousWhere.Connector, mainExpression, nextExpression);
                }

                // Save the previous where so the connector can be retrieved
                previousWhere = where;
            }

            return mainExpression ?? Expression.Constant(false);
        }

        #endregion

        #region Conditional Single

        /// <summary>
        /// Create a single predicate for the single set of supplied conditional arguments
        /// </summary>
        public static Expression<Func<T, bool>> BuildPredicate(string path, Comparison comparison, string?[]? values)
        {
            return BuildPredicate(path, comparison, values, null);
        }

        /// <summary>
        /// Create a single predicate for the single set of supplied conditional arguments
        /// </summary>
        public static Expression<Func<T, bool>> BuildPredicate(string path, Comparison comparison, string?[]? values, CaseString? stringComparison)
        {
            return BuildPredicate(path, comparison, values, false, stringComparison);
        }

        /// <summary>
        /// Create a single predicate for the single set of supplied conditional arguments
        /// </summary>
        public static Expression<Func<T, bool>> BuildPredicate(string path, Comparison comparison, string?[]? values, bool negate)
        {
            return BuildPredicate(path, comparison, values, negate, null);
        }

        /// <summary>
        /// Create a single predicate for the single set of supplied conditional arguments
        /// </summary>
        public static Expression<Func<T, bool>> BuildPredicate(string path, Comparison comparison, string?[]? values, bool negate, CaseString? stringComparison)
        {
            var expressionBody = MakePredicateBody(path, comparison, values, negate, stringComparison);
            var param = PropertyCache<T>.SourceParameter;

            return Expression.Lambda<Func<T, bool>>(expressionBody, param);
        }

        /// <summary>
        /// Makes the predicate body from the single set of supplied conditional arguments
        /// </summary>
        static Expression MakePredicateBody(string path, Comparison comparison, string?[]? values, bool negate = false, CaseString? stringComparison = null)
        {
            Expression expressionBody;

            // If path includes list property access
            if (HasListInPath(path))
            {
                // Handle a list path
                expressionBody = ProcessList(path, comparison, values!, stringComparison);
            }
            // Otherwise linear property access
            else
            {
                // Just get expression
                expressionBody = GetExpression(path, comparison, values, stringComparison);
            }

            // If the expression should be negated
            if (negate)
            {
                // Not it
                expressionBody = NegateExpression(expressionBody);
            }

            return expressionBody;
        }

        #endregion

        #region Body Builders (Lol)

        /// <summary>
        /// Process a list based item inside the property path
        /// </summary>
        static Expression ProcessList(string path, Comparison comparison, string?[]? values, CaseString? stringComparison = null)
        {
            // Get the path pertaining to individual list items
            var listPath = Regex.Match(path, LIST_PROPERTY_PATTERN).Groups[1].Value;
            // Remove the part of the path that leads into list item properties
            path = Regex.Replace(path, LIST_PROPERTY_PATTERN, "");

            // Get the property on the current object up to the list member
            var property = PropertyCache<T>.GetProperty(path);

            // Get the list item type details
            var listItemType = property.PropertyType.GetGenericArguments().Single();

            // Generate the predicate for the list item type
            var subPredicate = (Expression)typeof(ExpressionBuilderWhere<>)
                .MakeGenericType(listItemType)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == "BuildPredicate" && m.GetParameters().Length == 5)
                .Invoke(new(), new object[] { listPath, comparison, values!, false, stringComparison! })!;

            // Generate a method info for the Any Enumerable Static Method
            var anyInfo = typeof(Enumerable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                .MakeGenericMethod(listItemType);

            // Create Any Expression Call
            return Expression.Call(anyInfo, property.Left, subPredicate);
        }

        /// <summary>
        /// Build an expression from provided where parameters
        /// </summary>
        static Expression GetExpression(string path, Comparison comparison, string?[]? values, CaseString? stringComparison = null)
        {
            var property = PropertyCache<T>.GetProperty(path);
            Expression expressionBody;

            if (property.PropertyType == typeof(string))
            {
                switch (comparison)
                {
                    case Comparison.In:
                        WhereValidator.ValidateString(comparison, stringComparison);
                        expressionBody = MakeStringIn(values!, property, stringComparison);
                        break;

                    default:
                        WhereValidator.ValidateSingleString(comparison, stringComparison);
                        var value = values?.Single();
                        expressionBody = MakeStringComparison(comparison, value, property, stringComparison);
                        break;
                }
            }
            else
            {
                switch (comparison)
                {
                    case Comparison.In:
                        WhereValidator.ValidateObject(property.PropertyType, comparison, stringComparison);
                        expressionBody = MakeObjectIn(values!, property);
                        break;

                    default:
                        WhereValidator.ValidateSingleObject(property.PropertyType, comparison, null);
                        var value = values?.Single();
                        var valueObject = TypeConverter.ConvertStringToType(value, property.PropertyType);
                        expressionBody = MakeObjectComparison(comparison, valueObject, property);
                        break;
                }
            }

            return expressionBody;
        }

        #endregion

        #region Operations

        /// <summary>
        /// Make Object List In Comparision
        /// </summary>
        static Expression MakeObjectIn(string[] values, Property<T> property)
        {
            // Attempt to convert the string values to the object type
            var objects = TypeConverter.ConvertStringsToList(values, property.Info);
            // Make the object values a constant expression
            var constant = Expression.Constant(objects);
            // Build and return the expression body
            return Expression.Call(constant, property.ListContainsMethod!, property.Left);
        }

        /// <summary>
        /// Make String List In Comparison
        /// </summary>
        static Expression MakeStringIn(string[] values, Property<T> property, CaseString? comparison)
        {
            MethodCallExpression equalsBody;
            var left = property.Left;
            if (values is not null && comparison is not null && CaseString.Ignore == (CaseString)comparison)
            {
                values = Array.ConvertAll(values, d => d.ToLower());
                left = Expression.Call(left, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
            }

            equalsBody = Expression.Call(null, ReflectionCache.StringEqual, ExpressionCache.StringParam, left);

            // Make lambda for comparing each string value against property value
            var itemEvaluate = Expression.Lambda<Func<string, bool>>(equalsBody, ExpressionCache.StringParam);

            // Build Expression body to check if any string values match the property value
            return Expression.Call(null, ReflectionCache.StringAny, Expression.Constant(values), itemEvaluate);
        }

        /// <summary>
        /// Make String based single value comparisons
        /// </summary>
        static Expression MakeStringComparison(Comparison comparison, string? value, Property<T> property, CaseString? stringComparison)
        {
            var left = property.Left;

            Expression valueConstant = Expression.Constant(value, typeof(string));
            var nullCheck = Expression.NotEqual(left, ExpressionCache.Null);

            if (value is not null && stringComparison is not null && CaseString.Ignore == (CaseString)stringComparison)
            {
                left = Expression.Call(left, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
                valueConstant = Expression.Call(valueConstant, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
            }

            switch (comparison)
            {
                case Comparison.Equal:
                    var equalExpression = Expression.Call(ReflectionCache.StringEqual, left, valueConstant);
                    return value is null ?
                        equalExpression
                        :
                        Expression.AndAlso(nullCheck, equalExpression);
                //return Expression.Call(ReflectionCache.StringEqual, left, valueConstant);
                case Comparison.StartsWith:
                    var startsWithExpression = Expression.Call(left, ReflectionCache.StringStartsWith, valueConstant);
                    return Expression.AndAlso(nullCheck, startsWithExpression);
                case Comparison.EndsWith:
                    var endsWithExpression = Expression.Call(left, ReflectionCache.StringEndsWith, valueConstant);
                    return Expression.AndAlso(nullCheck, endsWithExpression);
                case Comparison.Contains:
                    var containsExpression = Expression.Call(left, ReflectionCache.StringContains, valueConstant);
                    var notEqualExpression = Expression.NotEqual(containsExpression, ExpressionCache.False);
                    return Expression.AndAlso(nullCheck, notEqualExpression);
                case Comparison.IndexOf:
                    var indexOfExpression = Expression.Call(left, ReflectionCache.StringIndexOf, valueConstant);
                    var indexOfNotEqualExpression = Expression.NotEqual(indexOfExpression, ExpressionCache.NegativeOne);
                    return Expression.AndAlso(nullCheck, indexOfNotEqualExpression);
            }

            throw new($"Invalid comparison operator '{comparison}'.");
        }

        /// <summary>
        /// Make Object based single value comparisons
        /// </summary>
        static Expression MakeObjectComparison(Comparison comparison, object? value, Property<T> property)
        {
            var left = property.Left;
            var constant = Expression.Constant(value, left.Type);

            switch (comparison)
            {
                case Comparison.Equal:
                    return Expression.MakeBinary(ExpressionType.Equal, left, constant);
                case Comparison.GreaterThan:
                    return Expression.MakeBinary(ExpressionType.GreaterThan, left, constant);
                case Comparison.GreaterThanOrEqual:
                    return Expression.MakeBinary(ExpressionType.GreaterThanOrEqual, left, constant);
                case Comparison.LessThan:
                    return Expression.MakeBinary(ExpressionType.LessThan, left, constant);
                case Comparison.LessThanOrEqual:
                    return Expression.MakeBinary(ExpressionType.LessThanOrEqual, left, constant);
            }

            throw new($"Invalid comparison operator '{comparison}'.");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Checks the path for matching list property marker
        /// </summary>
        private static bool HasListInPath(string path)
        {
            return Regex.IsMatch(path, LIST_PROPERTY_PATTERN);
        }

        /// <summary>
        /// Combine expressions by a specified binary operator
        /// </summary>
        static Expression CombineExpressions(Connector connector, Expression expr1, Expression expr2)
        {
            switch (connector)
            {
                case Connector.And:
                    return Expression.AndAlso(expr1, expr2);
                case Connector.Or:
                    return Expression.OrElse(expr1, expr2);
            }

            throw new($"Invalid connector operator '{connector}'.");
        }

        /// <summary>
        /// Negates a supplied expression
        /// </summary>
        static Expression NegateExpression(Expression expression)
        {
            return Expression.Not(expression);
        }

        #endregion
    }
}